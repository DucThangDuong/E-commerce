using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Queries
{
    public record GetMyVehiclesQuery(int CustomerId) : IRequest<Result<List<ResCustomerVehicleDto>>>;

    public class GetMyVehiclesHandler : IRequestHandler<GetMyVehiclesQuery, Result<List<ResCustomerVehicleDto>>>
    {
        private readonly IAppReadDbContext _db;

        public GetMyVehiclesHandler(IAppReadDbContext db)
        {
            _db = db;
        }

        public async Task<Result<List<ResCustomerVehicleDto>>> Handle(GetMyVehiclesQuery query, CancellationToken ct)
        {
            try
            {
                // Find all vehicles bought by the user in Completed orders
                var vehicles = await _db.OrderItems
                    .AsNoTracking()
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Vehicle)
                        .ThenInclude(v => v.Color)
                            .ThenInclude(c => c.Product)
                                .ThenInclude(p => p.ProductImages)
                    .Where(oi => oi.Order.CustomerId == query.CustomerId && oi.Order.Status == OrderStatus.Completed.ToString())
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

                return Result<List<ResCustomerVehicleDto>>.Success(vehicles);
            }
            catch (Exception ex)
            {
                return Result<List<ResCustomerVehicleDto>>.Failure($"Lỗi khi lấy danh sách xe: {ex.Message}", 500);
            }
        }
    }
}
