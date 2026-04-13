namespace Application.Interfaces
{
    public interface IUnitOfWork
    {
        ICustomerRepository CustomerRepository { get; }
        ICartRepository CartRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IBrandRepository BrandRepository { get; }
        IProductRepository ProductRepository { get; }
        IOrderRepository OrderRepository { get; }
        IInventoryRepository InventoryRepository { get; }
        IPaymentRepository PaymentRepository { get; }
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
