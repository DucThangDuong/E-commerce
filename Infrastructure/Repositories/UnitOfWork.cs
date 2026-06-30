using Application.Interfaces;
using Domain.Common;
using MediatR;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EcommerceContext _context;
        private readonly IPublisher _publisher;

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
            EcommerceContext context,
            IPublisher publisher,
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
            _publisher = publisher;
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
            var domainEntities = _context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, ct);
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}
