using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Carts.Commands
{
    public record AddItemCartCustomerCommand(int CustomerId, int ProductId, int Quantity) : IRequest<Result>;

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
                int? stockQuantity = await _unitOfWork.ProductRepository.GetStockQuantityAsync(command.ProductId, ct);

                if (stockQuantity == null)
                {
                    return Result.Failure("Product not found.", 404);
                }

                var existingCart = await _unitOfWork.CartRepository.GetCartAsync(command.CustomerId, command.ProductId);
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
                        ProductId = command.ProductId,
                        Quantity = command.Quantity
                    };
                    await _unitOfWork.CartRepository.AddNewCartAsync(newCart);
                }

                await _unitOfWork.SaveChangesAsync(ct);
                return Result.Success(201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart. CustomerId: {CustomerId}, ProductId: {ProductId}", command.CustomerId, command.ProductId);
                return Result.Failure("An internal error occurred while processing your request.", 500);
            }
        }
    }
}
