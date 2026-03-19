using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Carts.Commands
{
    public record DeleteItemCartCustomerCommand(int CustomerId, int ProductId) : IRequest<Result>;

    public class DeleteItemCartCustomerHandler : IRequestHandler<DeleteItemCartCustomerCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteItemCartCustomerHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteItemCartCustomerCommand command, CancellationToken ct)
        {
            bool result = await _unitOfWork.CartRepository.DeleteCartAsync(command.CustomerId, command.ProductId);
            if (result)
            {
                return Result.Success();
            }
            return Result.Failure("Failed to delete cart item.", 500);
        }
    }
}
