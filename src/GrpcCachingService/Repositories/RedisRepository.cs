
using GrpcCachingService.Repositories.Interfaces;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using System.Net.Sockets;

namespace GrpcCachingService.Repositories;

public class RedisRepository : IRedisRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _cache;
    private readonly ILogger<RedisRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public RedisRepository(IConnectionMultiplexer redis, ILogger<RedisRepository> logger)
    {
        _redis = redis;
        _cache = redis.GetDatabase();
        _logger = logger;
        _retryPolicy = Policy
            .Handle<RedisConnectionException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) =>
                {
                    _logger.LogWarning($"Redis operation failed. Retrying in {time.TotalSeconds} seconds. Error: {ex.Message}");
                });
    }

    public async Task<string> GetRecordAsync(string key)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var value = await _cache.StringGetAsync(key);
                return value.HasValue ? value.ToString() : string.Empty;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in {nameof(GetRecordAsync)}: {ex.Message}");
            return string.Empty;
        }
    }
    public async Task<bool> SetRecordAsync(string key, string data, TimeSpan? expiry = null)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _cache.StringSetAsync(key, data, expiry);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in {nameof(SetRecordAsync)}: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> DeleteRecordAsync(string key)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _cache.KeyDeleteAsync(key);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in {nameof(DeleteRecordAsync)}: {ex.Message}");
            return false;
        }
    }
}
