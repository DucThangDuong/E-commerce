using Application.Interfaces;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EcommerceOrderSystemContext _context;

        public ICustomerRepository CustomerRepository { get; }
        public ICartRepository CartRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        public IBrandRepository BrandRepository { get; }
        public IProductRepository ProductRepository { get; }
        public IOrderRepository OrderRepository { get; }
        public IInventoryRepository InventoryRepository { get; }
        public IPaymentRepository PaymentRepository { get; }
        public IOrderShippingDetailRepository OrderShippingDetailRepository { get; }

        public UnitOfWork(
            EcommerceOrderSystemContext context,
            ICustomerRepository customerRepository,
            ICartRepository cartRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IInventoryRepository inventoryRepository,
            IPaymentRepository paymentRepository,
            IOrderShippingDetailRepository orderShippingDetailRepository)
        {
            _context = context;
            CustomerRepository = customerRepository;
            CartRepository = cartRepository;
            CategoryRepository = categoryRepository;
            BrandRepository = brandRepository;
            ProductRepository = productRepository;
            OrderRepository = orderRepository;
            InventoryRepository = inventoryRepository;
            PaymentRepository = paymentRepository;
            OrderShippingDetailRepository = orderShippingDetailRepository;

        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
