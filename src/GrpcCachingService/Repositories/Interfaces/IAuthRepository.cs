namespace GrpcCachingService.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<bool> StoreRefreshTokenAsync(string jwtId, string refreshToken, int expiry);
    
    Task<bool> RemoveRefreshTokenAsync(string refreshToken);
    
    Task<string> GetJwtIdForRefreshTokenAsync(string refreshToken);
}