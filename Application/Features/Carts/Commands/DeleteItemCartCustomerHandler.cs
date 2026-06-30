using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Carts.Commands
{
    public record DeleteItemCartCustomerCommand(int CustomerId, int ColorId) : IRequest<Result>;

    public class DeleteItemCartCustomerHandler : IRequestHandler<DeleteItemCartCustomerCommand, Result>
    {

        private readonly ICartRepository _cartRepository;
        public DeleteItemCartCustomerHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<Result> Handle(DeleteItemCartCustomerCommand command, CancellationToken ct)
        {
            try
            {
                bool result = await _cartRepository.DeleteCartAsync(command.CustomerId, command.ColorId);
                if (result)
                {
                    return Result.Success(204);
                }
                return Result.Failure("Failed to delete cart item.", 500);
            }
            catch (Exception ex)
            {
                return Result.Failure("An internal error occurred while deleting the cart item.", 500);
            }
        }
    }
}
