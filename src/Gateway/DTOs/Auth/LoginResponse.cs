namespace Gateway.DTOs.Auth;

public class LoginResponse
{
    public string AccessToken { get; init; }
    
    public string RefreshToken { get; init; }
}