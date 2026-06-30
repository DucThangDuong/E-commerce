using API.Extensions;
using Application.DTOs.Response;
using Application.Common;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Customer
{
    public class GetCustomerProfileEndpoint : EndpointWithoutRequest
    {
        private readonly IAppReadDbContext _db;
        
        public GetCustomerProfileEndpoint(IAppReadDbContext db)
        {
            _db = db;
        }

        public override void Configure()
        {
            Get("/customer/profile");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            
            var customerDb = await _db.Customers
                .AsNoTracking()
                .Include(x => x.Orders)
                .Where(x => x.CustomerId == userId)
                .FirstOrDefaultAsync(ct);

            if (customerDb == null)
            {
                await this.SendApiResponseAsync(Result<ResCustomerPrivateDto>.Failure("Not found", 404), ct);
                return;
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

            await this.SendApiResponseAsync(Result<ResCustomerPrivateDto>.Success(customer), ct);
        }
    }
}
