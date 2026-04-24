using Domain.Entities.Common;
using Domain.Events;
using FluentAssertions;

namespace Domain.Tests.Entities
{
    public class BaseEntityTests
    {
        // Concrete implementation for testing the abstract BaseEntity
        private sealed class TestEntity : BaseEntity { }

        private sealed record TestDomainEvent : BaseDomainEvent;

        [Fact]
        public void RaiseDomainEvent_AddsEventToList()
        {
            var entity = new TestEntity();
            var domainEvent = new TestDomainEvent();

            entity.RaiseDomainEvent(domainEvent);

            entity.DomainEvents.Should().ContainSingle()
                .Which.Should().Be(domainEvent);
        }

        [Fact]
        public void ClearDomainEvents_RemovesAllEvents()
        {
            var entity = new TestEntity();
            entity.RaiseDomainEvent(new TestDomainEvent());
            entity.RaiseDomainEvent(new TestDomainEvent());

            entity.ClearDomainEvents();

            entity.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void DomainEvents_ReturnsReadOnlyList()
        {
            var entity = new TestEntity();

            entity.DomainEvents.Should().BeAssignableTo<IReadOnlyList<IDomainEvent>>();
        }
    }
}
