using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Queries
{
    public record GetCustomerProfileQuery(int CustomerId) : IRequest<Result<ResCustomerPrivateDto>>;

    public class GetCustomerProfileHandler : IRequestHandler<GetCustomerProfileQuery, Result<ResCustomerPrivateDto>>
    {
        private readonly IAppReadDbContext _db;

        public GetCustomerProfileHandler(IAppReadDbContext db)
        {
            _db = db;
        }

        public async Task<Result<ResCustomerPrivateDto>> Handle(GetCustomerProfileQuery query, CancellationToken ct)
        {
            try
            {
                var customer = await _db.Customers
                    .AsNoTracking()
                    .Where(x => x.CustomerId == query.CustomerId)
                    .Select(x => new ResCustomerPrivateDto
                    {
                        avatarUrl = x.CustomAvatar,
                        email = x.Email,
                        id = x.CustomerId,
                        name = x.Name,
                        address = x.Address,
                        phoneNumber = x.PhoneNumber,
                    })
                    .FirstOrDefaultAsync(ct);

                if (customer == null)
                {
                    return Result<ResCustomerPrivateDto>.Failure("Not found", 404);
                }
                return Result<ResCustomerPrivateDto>.Success(customer);
            }
            catch (Exception ex)
            {
                return Result<ResCustomerPrivateDto>.Failure(ex.Message, 400);
            }
        }
    }
}
