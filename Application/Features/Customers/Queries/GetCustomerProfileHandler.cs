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
                var customerDb = await _db.Customers
                    .AsNoTracking()
                    .Include(x => x.Orders)
                    .Where(x => x.CustomerId == query.CustomerId)
                    .FirstOrDefaultAsync(ct);

                if (customerDb == null)
                {
                    return Result<ResCustomerPrivateDto>.Failure("Not found", 404);
                }

                int completedOrders = customerDb.Orders?.Count(o => o.Status == "Completed") ?? 0;
                string? maskedPhone = customerDb.PhoneNumber;
                if (!string.IsNullOrEmpty(maskedPhone) && maskedPhone.Length >= 6)
                {
                    maskedPhone = maskedPhone.Substring(0, 3) + new string('*', maskedPhone.Length - 6) + maskedPhone.Substring(maskedPhone.Length - 3);
                }

                var customer = new ResCustomerPrivateDto
                {
                    avatarUrl = customerDb.CustomAvatar ?? customerDb.GoogleAvatar,
                    email = customerDb.Email,   
                    id = customerDb.CustomerId,
                    name = customerDb.Name,
                    address = customerDb.Address,
                    phoneNumber = customerDb.PhoneNumber,
                    maskedPhoneNumber = maskedPhone,
                    isGoogleLinked = !string.IsNullOrEmpty(customerDb.GoogleId),
                    totalOrders = customerDb.Orders?.Count() ?? 0
                };

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
