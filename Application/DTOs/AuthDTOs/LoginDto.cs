namespace Application.DTOs.AuthDTOs
{
    public class LoginDto
    {
        public Guid UserId { get; set; }
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiryRefreshTokenTime { get; set; }
    }
}
