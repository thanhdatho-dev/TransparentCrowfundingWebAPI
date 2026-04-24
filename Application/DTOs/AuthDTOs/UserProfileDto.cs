namespace Application.DTOs.AuthDTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string WalletAddress { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
