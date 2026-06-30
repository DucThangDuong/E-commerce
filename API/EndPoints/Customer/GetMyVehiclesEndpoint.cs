using API.Extensions;
using Application.DTOs.Response;
using Application.Common;
using Application.Interfaces;
using Domain.Enums;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Customer
{
    public class GetMyVehiclesEndpoint : EndpointWithoutRequest
    {
        private readonly IAppReadDbContext _db;
        
        public GetMyVehiclesEndpoint(IAppReadDbContext db)
        {
            _db = db;
        }

        public override void Configure()
        {
            Get("/customer/my-vehicles");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            
            var vehicles = await _db.OrderItems
                .AsNoTracking()
                .Include(oi => oi.Order)
                .Include(oi => oi.Vehicle)
                    .ThenInclude(v => v.Color)
                        .ThenInclude(c => c.Product)
                            .ThenInclude(p => p.ProductImages)
                .Where(oi => oi.Order.CustomerId == userId && oi.Order.Status == OrderStatus.Completed.ToString())
                .Select(oi => new ResCustomerVehicleDto
                {
                    VehicleId = oi.VehicleId,
                    ProductName = oi.Vehicle.Color.Product.Name,
                    ColorName = oi.Vehicle.Color.ColorName,
                    Vin = oi.Vehicle.Vin,
                    EngineNumber = oi.Vehicle.EngineNumber,
                    LicensePlate = "Đang cập nhật",
                    PurchaseDate = oi.Order.OrderDate,
                    NextMaintenanceDate = oi.Order.OrderDate != null ? oi.Order.OrderDate.Value.AddMonths(6) : DateTime.UtcNow.AddMonths(6),
                    ImageUrl = oi.Vehicle.Color.Product.ProductImages.Where(pi => pi.ColorId == null || pi.ColorId == oi.Vehicle.ColorId).Select(pi => pi.ImageUrl).FirstOrDefault()
                })
                .OrderByDescending(v => v.PurchaseDate)
                .ToListAsync(ct);

            await this.SendApiResponseAsync(Result<List<ResCustomerVehicleDto>>.Success(vehicles), ct);
        }
    }
}
