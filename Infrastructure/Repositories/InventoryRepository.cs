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
            return await _context.Inventories
                .Where(i => i.Color.ProductId == productId)
                .SumAsync(i => i.StockQuantity);
        }

        public async Task<Dictionary<int, int>> GetStockByColorIdsAsync(List<int> colorIds, CancellationToken ct = default)
        {
            return await _context.Inventories
                .AsNoTracking()
                .Where(i => colorIds.Contains(i.ColorId))
                .ToDictionaryAsync(i => i.ColorId, i => i.StockQuantity, ct);
        }

        public async Task<bool> UpdateDecreaseStockAsync(Dictionary<int, int>? purchasedItems, CancellationToken ct = default)
        {
            if (purchasedItems != null)
            {
                var colorIds = purchasedItems.Keys.ToList();
                var inventories = await _context.Inventories
                    .Where(i => colorIds.Contains(i.ColorId))
                    .ToListAsync(ct);
                foreach (var inventory in inventories)
                {
                    if (purchasedItems.TryGetValue(inventory.ColorId, out int purchasedQuantity))
                    {
                        inventory.StockQuantity -= purchasedQuantity;
                    }
                }
            }
            return true;
        }
    }
}
