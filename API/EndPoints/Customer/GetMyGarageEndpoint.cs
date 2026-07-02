using API.Extensions;
using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Customer
{
    public class GetMyGarageEndpoint : EndpointWithoutRequest
    {
        private readonly IAppReadDbContext _db;

        public GetMyGarageEndpoint(IAppReadDbContext db)
        {
            _db = db;
        }

        public override void Configure()
        {
            Get("/customer/garage");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            int customerId = HttpContext.User.GetUserId();

            var vehicles = await _db.WarrantyBooks
                .AsNoTracking()
                .Where(w => w.CustomerId == customerId)
                .Select(w => new GarageVehicleDto
                {
                    Id = "V-" + w.VehicleId,
                    Name = w.Vehicle.Color.Product.Name,
                    Color = w.Vehicle.Color.ColorName,
                    Vin = w.Vehicle.Vin,
                    EngineNumber = w.Vehicle.EngineNumber,
                    PurchaseDate = w.ActivatedAt,
                    WarrantyUntil = w.ValidUntil,
                    Status = w.ValidUntil >= DateTime.UtcNow ? "active" : "expired",
                    Image = w.Vehicle.Color.Product.ProductImages
                                .Where(pi => pi.ColorId == null || pi.ColorId == w.Vehicle.ColorId)
                                .Select(pi => pi.ImageUrl)
                                .FirstOrDefault() ?? "",
                    Benefits = new List<string> {
                        "Bảo hành chính hãng 3 năm",
                        "Cứu hộ miễn phí 24/7 (1 năm)",
                        "Bảo dưỡng định kỳ 6 lần"
                    }
                })
                .ToListAsync(ct);

            await this.SendApiResponseAsync(Result<List<GarageVehicleDto>>.Success(vehicles), ct);
        }
    }
}
