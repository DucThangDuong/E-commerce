using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Customers.Queries
{
    public record GetCustomerProfileQuery(int CustomerId) : IRequest<Result<ResCustomerPrivate>>;

    public class GetCustomerProfileHandler : IRequestHandler<GetCustomerProfileQuery, Result<ResCustomerPrivate>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCustomerProfileHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ResCustomerPrivate>> Handle(GetCustomerProfileQuery query, CancellationToken ct)
        {
            try
            {
                var customer = await _unitOfWork.CustomerRepository.GetCustomerProfileAsync(query.CustomerId, ct);
                if (customer == null)
                {
                    return Result<ResCustomerPrivate>.Failure("Not found", 404);
                }
                return Result<ResCustomerPrivate>.Success(customer);
            }
            catch (Exception ex)
            {
                return Result<ResCustomerPrivate>.Failure(ex.Message, 400);
            }
        }
    }
}
