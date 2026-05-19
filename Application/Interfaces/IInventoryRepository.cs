using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IInventoryRepository
    {
        public Task<int> GetStockQuantity(int productId);
        Task<Dictionary<int, int>> GetStockByColorIdsAsync(List<int> colorIds, CancellationToken ct = default);
        Task<bool> UpdateDecreaseStockAsync(Dictionary<int, int>? purchasedItems, CancellationToken ct = default);
    }
}
