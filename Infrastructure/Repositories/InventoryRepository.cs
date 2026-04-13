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
        public readonly EcommerceOrderSystemContext _context;
        public InventoryRepository(EcommerceOrderSystemContext context) { 
            _context = context;
        }
        public async Task<int> GetStockQuantity(int productId)
        {
            return await _context.Inventories
                .Where(i => i.ProductId == productId)
                .Select(i => i.StockQuantity)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, int>> GetStockByProductIdsAsync(List<int> productIds, CancellationToken ct = default)
        {
            return await _context.Inventories
                .AsNoTracking()
                .Where(i => productIds.Contains(i.ProductId))
                .ToDictionaryAsync(i => i.ProductId, i => i.StockQuantity, ct);
        }

        public async Task<bool> UpdateDecreaseStockAsync(Dictionary<int, int> purchasedItems, CancellationToken ct = default)
        {
            var productIds = purchasedItems.Keys.ToList();
            var inventories = await _context.Inventories
                .Where(i => productIds.Contains(i.ProductId))
                .ToListAsync(ct);
            var updatedStock = new Dictionary<int, int>();
            foreach (var inventory in inventories)
            {
                if (purchasedItems.TryGetValue(inventory.ProductId, out int purchasedQuantity))
                {
                    inventory.StockQuantity -= purchasedQuantity;
                }
            }
            return true;
        }
    }
}
