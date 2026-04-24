using Domain.Entities.Common;
using Domain.Interfaces.Repositories;
using Infrastructure.Contexts;
using MediatR;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IPublisher _publisher;

        public IUserRepository Users { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IUserRepository users,
            IPublisher publisher)
        {
            _context = context;
            Users = users;
            _publisher = publisher;
        }

        public async Task<int> SaveChangesAsync()
        {
            var result = await _context.SaveChangesAsync().ConfigureAwait(false);

            // Dispatch domain events sau khi lưu DB thành công
            await DispatchDomainEventsAsync();

            return result;
        }

        private async Task DispatchDomainEventsAsync()
        {
            var entities = _context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Count > 0)
                .Select(e => e.Entity)
                .ToList();

            var events = entities
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear trước khi publish để tránh infinite loop
            entities.ForEach(e => e.ClearDomainEvents());

            foreach (var domainEvent in events)
                await _publisher.Publish(domainEvent);
        }

        public void Dispose() => _context.Dispose();
    }
}

