using Application.Interfaces;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EcommerceOrderSystemContext _context;

        public ICustomerRepository CustomerRepository { get; }
        public ICartRepository CartRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        public IProductRepository ProductRepository { get; }

        public UnitOfWork(
            EcommerceOrderSystemContext context,
            ICustomerRepository customerRepository,
            ICartRepository cartRepository,
            ICategoryRepository categoryRepository,
            IProductRepository productRepository)
        {
            _context = context;
            CustomerRepository = customerRepository;
            CartRepository = cartRepository;
            CategoryRepository = categoryRepository;
            ProductRepository = productRepository;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
