using MediatR;

namespace Domain.Events
{
    /// <summary>
    /// Marker interface cho tất cả Domain Events.
    /// Kế thừa INotification để dùng MediatR publish.
    /// </summary>
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}
