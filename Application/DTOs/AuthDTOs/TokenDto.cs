namespace Application.DTOs.AuthDTOs
{
    public class TokenDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime RefreshTokenExiryTime { get; set; }
    }
}
