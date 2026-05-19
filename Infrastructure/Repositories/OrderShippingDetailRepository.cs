using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories
{
    public class OrderShippingDetailRepository : IOrderShippingDetailRepository
    {
        private readonly EcommerceContext _context;

        public OrderShippingDetailRepository(EcommerceContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OrderShippingDetail shippingDetail)
        {
            await _context.OrderShippingDetails.AddAsync(shippingDetail);
        }
    }
}
