using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Carts.Queries
{
    public record GetItemCartCustomerQuery(int CustomerId) : IRequest<Result<List<ResCartDto>>>;

    public class GetItemCartCustomerHandler : IRequestHandler<GetItemCartCustomerQuery, Result<List<ResCartDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetItemCartCustomerHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<ResCartDto>>> Handle(GetItemCartCustomerQuery query, CancellationToken ct)
        {
            try
            {
                var result = await _unitOfWork.CartRepository.GetCartItemsByCustomerIdAsync(query.CustomerId, ct);
                return Result<List<ResCartDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<ResCartDto>>.Failure(ex.Message);
            }
        }
    }
}
