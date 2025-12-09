namespace Gateway.DTOs.Auth;

public class LoginRequest
{
    public string NeptunCode { get; init; }
    
    public string Password { get; init; }
}