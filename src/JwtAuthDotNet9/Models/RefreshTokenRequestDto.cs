namespace JwtAuthDotNet9.Models
{
    public class RefreshTokenRequestDto
    {
        public Guid UserID { get; set; }
        public required string RefreshToken { get; set; }
    }
}
