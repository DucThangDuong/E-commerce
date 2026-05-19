using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Carts.Commands
{
    public record AddItemCartCustomerCommand(int CustomerId, int ColorId, int Quantity) : IRequest<Result>;

    public class AddItemCartCustomerHandler : IRequestHandler<AddItemCartCustomerCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddItemCartCustomerHandler> _logger;

        public AddItemCartCustomerHandler(IUnitOfWork unitOfWork, ILogger<AddItemCartCustomerHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(AddItemCartCustomerCommand command, CancellationToken ct)
        {
            try
            {
                var dbStockMap = await _unitOfWork.InventoryRepository.GetStockByColorIdsAsync(new List<int> { command.ColorId }, ct);
                int? stockQuantity = dbStockMap.ContainsKey(command.ColorId) ? dbStockMap[command.ColorId] : null;

                if (stockQuantity == null)
                {
                    return Result.Failure("Variant not found.", 404);
                }

                var existingCart = await _unitOfWork.CartRepository.GetCartAsync(command.CustomerId, command.ColorId);
                int currentQuantityInCart = existingCart?.Quantity ?? 0;

                if (stockQuantity.Value == 0 || (currentQuantityInCart + command.Quantity) > stockQuantity.Value)
                {
                    return Result.Failure("Not enough stock available for the requested quantity.", 400);
                }

                if (existingCart != null)
                {
                    existingCart.Quantity += command.Quantity;
                }
                else
                {
                    Cart newCart = new Cart
                    {
                        CustomerId = command.CustomerId,
                        ColorId = command.ColorId,
                        Quantity = command.Quantity
                    };
                    await _unitOfWork.CartRepository.AddNewCartAsync(newCart);
                }

                await _unitOfWork.SaveChangesAsync(ct);
                return Result.Success(201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart. CustomerId: {CustomerId}, ColorId: {ColorId}", command.CustomerId, command.ColorId);
                return Result.Failure("An internal error occurred while processing your request.", 500);
            }
        }
    }
}
