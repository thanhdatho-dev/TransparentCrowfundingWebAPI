namespace Domain.Events
{
    /// <summary>
    /// Base record cho Domain Events. Dùng record vì event là immutable data.
    /// </summary>
    public abstract record BaseDomainEvent : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
