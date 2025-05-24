namespace GrpcAuthService.Options;

public class JwtOptions
{
    public string Issuer { get; set; }
    
    public string Secret { get; set; }
    
    public TimeSpan AccessTokenLifetime { get; set; }
    
    public int RefreshTokenLifetime { get; set; }
}