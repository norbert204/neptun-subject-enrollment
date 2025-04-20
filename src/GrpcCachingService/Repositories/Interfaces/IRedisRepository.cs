namespace GrpcCachingService.Repositories.Interfaces;

public interface IRedisRepository
{
    Task<string> GetRecordAsync(string key);
    Task<bool> SetRecordAsync(string key, string data, TimeSpan? expiry = null);
    Task<bool> DeleteRecordAsync(string key);
}