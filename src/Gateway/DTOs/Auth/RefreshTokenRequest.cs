namespace Gateway.DTOs.Auth;

public class RefreshTokenRequest
{
    public string AccessToken { get; init; }
    
    public string RefreshToken { get; init; }
}