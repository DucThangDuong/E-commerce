using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Carts.Commands
{
    public record UpdateItemCartCustomerCommand(int CustomerId, int CartId, int Quantity) : IRequest<Result<ResCartDto>>;

    public class UpdateItemCartCustomerHandler : IRequestHandler<UpdateItemCartCustomerCommand, Result<ResCartDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppReadDbContext _db;
        private readonly ILogger<UpdateItemCartCustomerHandler> _logger;

        public UpdateItemCartCustomerHandler(IUnitOfWork unitOfWork, IAppReadDbContext db, ILogger<UpdateItemCartCustomerHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _logger = logger;
        }

        public async Task<Result<ResCartDto>> Handle(UpdateItemCartCustomerCommand command, CancellationToken ct)
        {
            try
            {
                var existingCart = await _unitOfWork.CartRepository.GetCartByIdAsync(command.CartId, command.CustomerId);
                if (existingCart == null)
                {
                    return Result<ResCartDto>.Failure("Cart item not found.", 404);
                }

                if (command.Quantity <= 0)
                {
                    await _unitOfWork.CartRepository.DeleteCartByIdAsync(command.CartId, command.CustomerId);
                    await _unitOfWork.SaveChangesAsync(ct);
                    // Return a result with quantity 0 to indicate deletion
                    return Result<ResCartDto>.Success(new ResCartDto { CartId = command.CartId, Quantity = 0 }, 200);
                }

                var dbStockMap = await _unitOfWork.InventoryRepository.GetStockByColorIdsAsync(new List<int> { existingCart.ColorId }, ct);
                int? stockQuantity = dbStockMap.ContainsKey(existingCart.ColorId) ? dbStockMap[existingCart.ColorId] : null;

                if (stockQuantity == null)
                {
                    return Result<ResCartDto>.Failure("Variant not found.", 404);
                }

                if (command.Quantity > stockQuantity.Value)
                {
                    return Result<ResCartDto>.Failure($"Not enough stock available. Maximum is {stockQuantity.Value}.", 400);
                }

                existingCart.Quantity = command.Quantity;
                await _unitOfWork.SaveChangesAsync(ct);

                // Fetch the updated full DTO
                var result = await _db.Carts
                    .AsNoTracking()
                    .Where(e => e.CartId == command.CartId)
                    .Select(e => new ResCartDto
                    {
                        BasePrice = e.Color.Product.BasePrice,
                        CartId = e.CartId,
                        CategoryId = e.Color.Product.CategoryId,
                        Description = e.Color.Product.Description,
                        Name = e.Color.Product.Name,
                        ProductId = e.Color.ProductId,
                        ColorId = e.ColorId,
                        ColorName = e.Color.ColorName,
                        Quantity = e.Quantity,
                        StockQuantity = e.Color.Inventory != null ? e.Color.Inventory.StockQuantity : 0,
                        imageUrl = e.Color.Product.ProductImages.Where(pi => pi.ColorId == null || pi.ColorId == e.ColorId).Select(pi => pi.ImageUrl).ToList(),
                    })
                    .FirstOrDefaultAsync(ct);

                return Result<ResCartDto>.Success(result!, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart. CustomerId: {CustomerId}, CartId: {CartId}", command.CustomerId, command.CartId);
                return Result<ResCartDto>.Failure("An internal error occurred while processing your request.", 500);
            }
        }
    }
}
