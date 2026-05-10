using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories
{
    public class OrderShippingDetailRepository : IOrderShippingDetailRepository
    {
        private readonly EcommerceOrderSystemContext _context;

        public OrderShippingDetailRepository(EcommerceOrderSystemContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OrderShippingDetail shippingDetail)
        {
            await _context.OrderShippingDetails.AddAsync(shippingDetail);
        }
    }
}
