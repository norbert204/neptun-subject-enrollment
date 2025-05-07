using Microsoft.Identity.Client;

namespace JwtAuthDotNet9.Models
{
    public class TokenResponseDto
    {
        public required string AccessToken {  get; set; }
        public required string RefreshToken { get; set; }
    }
}
