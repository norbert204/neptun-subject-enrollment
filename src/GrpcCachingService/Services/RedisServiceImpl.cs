using Grpc.Core;
using GrpcCachingService.Repositories.Interfaces;

namespace GrpcCachingService.Services;

public class RedisServiceImpl : RedisService.RedisServiceBase
{
    private readonly IRedisRepository _redisRepository;
    private readonly ILogger<RedisServiceImpl> _logger;

    public RedisServiceImpl(IRedisRepository redisRepository, ILogger<RedisServiceImpl> logger)
    {
        _redisRepository = redisRepository;
        _logger = logger;
    }

    public override async Task<GetResponse> GetValue(GetRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Getting value for key: {request.Key}");

        var data = await _redisRepository.GetRecordAsync(request.Key);

        return new GetResponse
        {
            Data = data,
            Success = !string.IsNullOrEmpty(data)
        };
    }

    public override async Task<SetResponse> SetValue(SetRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Setting value for key: {request.Key}");

        TimeSpan? expiry = request.ExpirySeconds > 0
            ? TimeSpan.FromSeconds(request.ExpirySeconds)
            : null;

        var success = await _redisRepository.SetRecordAsync(request.Key, request.Data, expiry);

        return new SetResponse
        {
            Success = success
        };
    }

    public override async Task<DeleteResponse> DeleteValue(DeleteRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Deleting value for key: {request.Key}");

        var success = await _redisRepository.DeleteRecordAsync(request.Key);

        return new DeleteResponse
        {
            Success = success
        };
    }
}
