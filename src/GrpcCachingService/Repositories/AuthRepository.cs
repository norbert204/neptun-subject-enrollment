using GrpcCachingService.Repositories.Interfaces;
using StackExchange.Redis;

namespace GrpcCachingService.Repositories;

public class AuthRepository : IAuthRepository
{
    private const string KeyPrefix = "refreshToken";
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    
    public AuthRepository(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _database = _connectionMultiplexer.GetDatabase();
    }

    public Task<bool> StoreRefreshTokenAsync(string jwtId, string refreshToken, int expiry) =>
        _database.StringSetAsync($"{KeyPrefix}:{refreshToken}", jwtId, TimeSpan.FromDays(expiry));

    public Task<bool> RemoveRefreshTokenAsync(string refreshToken) =>
        _database.KeyDeleteAsync($"{KeyPrefix}:{refreshToken}");

    public async Task<string> GetJwtIdForRefreshTokenAsync(string refreshToken)
    {
        var result = await _database.StringGetAsync($"{KeyPrefix}:{refreshToken}");

        if (!result.HasValue)
        {
            return null;
        }
        
        return result.ToString();
    }
}