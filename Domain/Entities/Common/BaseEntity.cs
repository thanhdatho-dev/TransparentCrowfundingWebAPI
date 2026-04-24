using Domain.Events;

namespace Domain.Entities.Common
{
    public abstract class BaseEntity
    {
        public virtual Guid Id { get; set; }

        private readonly List<IDomainEvent> _domainEvents = [];

        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void RaiseDomainEvent(IDomainEvent domainEvent)
            => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents()
            => _domainEvents.Clear();
    }
}

