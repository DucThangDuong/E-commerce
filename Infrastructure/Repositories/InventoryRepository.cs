using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        public readonly EcommerceContext _context;
        public InventoryRepository(EcommerceContext context) { 
            _context = context;
        }
        public async Task<int> GetStockQuantity(int productId)
        {
            return await _context.Vehicles
                .Where(v => v.Color.ProductId == productId && v.Status == "Available")
                .CountAsync();
        }

        public async Task<Dictionary<int, int>> GetStockByColorIdsAsync(List<int> colorIds, CancellationToken ct = default)
        {
            return await _context.Vehicles
                .AsNoTracking()
                .Where(v => colorIds.Contains(v.ColorId) && v.Status == "Available")
                .GroupBy(v => v.ColorId)
                .Select(g => new { ColorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ColorId, x => x.Count, ct);
        }

        public async Task<List<Domain.Entities.Vehicle>> ReserveVehiclesAsync(Dictionary<int, int>? purchasedItems, CancellationToken ct = default)
        {
            var reservedVehicles = new List<Domain.Entities.Vehicle>();
            if (purchasedItems != null)
            {
                foreach (var item in purchasedItems)
                {
                    int colorId = item.Key;
                    int quantity = item.Value;

                    var vehiclesToReserve = await _context.Vehicles
                        .Where(v => v.ColorId == colorId && v.Status == "Available")
                        .Take(quantity)
                        .ToListAsync(ct);

                    if (vehiclesToReserve.Count < quantity)
                    {
                        throw new Exception($"Not enough stock for ColorId {colorId}.");
                    }

                    foreach (var v in vehiclesToReserve)
                    {
                        v.Status = "Reserved";
                        reservedVehicles.Add(v);
                    }
                }
            }
            return reservedVehicles;
        }

        public async Task ReleaseVehiclesAsync(List<int> vehicleIds, CancellationToken ct = default)
        {
            var vehicles = await _context.Vehicles.Where(v => vehicleIds.Contains(v.VehicleId)).ToListAsync(ct);
            foreach (var v in vehicles)
            {
                v.Status = "Available";
            }
        }
    }
}
