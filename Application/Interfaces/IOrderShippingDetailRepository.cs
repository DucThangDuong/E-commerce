using Domain.Entities;

namespace Application.Interfaces
{
    public interface IOrderShippingDetailRepository
    {
        Task AddAsync(OrderShippingDetail shippingDetail);
    }
}
