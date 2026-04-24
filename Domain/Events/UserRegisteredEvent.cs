namespace Domain.Events
{
    /// <summary>
    /// Raised khi user hoàn tất đăng ký (confirm email thành công).
    /// </summary>
    public sealed record UserRegisteredEvent(
        Guid UserId,
        string Email,
        string WalletAddress
    ) : BaseDomainEvent;
}
