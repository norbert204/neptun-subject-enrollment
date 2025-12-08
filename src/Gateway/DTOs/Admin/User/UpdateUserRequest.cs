namespace Gateway.DTOs.Admin.User;

public class UpdateUserRequest
{
    public string NeptunCode { get; init; }
    
    public string Name { get; init; }
    
    public string Email { get; init; }
    
    public string Password { get; init; }
}