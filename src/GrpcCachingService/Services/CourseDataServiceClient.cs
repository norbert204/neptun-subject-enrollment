using CourseRegistrationService.External;
using Grpc.Net.Client;
using GrpcCachingService.Services.Interfaces;
using Microsoft.Extensions.Options;
using Polly;

namespace GrpcCachingService.Services;
public class CourseDataServiceOptions
{
    public string ServiceUrl { get; set; } = "https://localhost:1234";
}
public class CourseDataServiceClient : ICourseDataServiceClient
{
    private readonly ILogger<CourseDataServiceClient> _logger;
    private readonly CourseDataServiceOptions _options;
    private readonly AsyncPolicy _retryPolicy;

    public CourseDataServiceClient(IOptions<CourseDataServiceOptions> options, ILogger<CourseDataServiceClient> logger)
    {
        _options = options.Value;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<Grpc.Core.RpcException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) =>
                {
                    _logger.LogWarning($"Failed to call external service. Retrying in {time.TotalSeconds} seconds. Error: {ex.Message}");
                });
    }

    public async Task<GetAllCoursesResponse> GetAllCoursesAsync()
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Fetching courses from external service: {_options.ServiceUrl}");

                using var channel = GrpcChannel.ForAddress(_options.ServiceUrl);
                var client = new CourseDataService.CourseDataServiceClient(channel);

                return await client.GetAllCoursesAsync(new GetAllCoursesRequest());
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while calling external service: {ex.Message}");

            return new GetAllCoursesResponse
            {
                Success = false,
                Message = $"Failed to connect to external service: {ex.Message}"
            };

        }
    }

    public async Task<GetStudentsResponse> GetStudentsWithEligibleCoursesAsync()
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Fetching students and their eligible courses from external service: {_options.ServiceUrl}");

                using var channel = GrpcChannel.ForAddress(_options.ServiceUrl);
                var client = new CourseDataService.CourseDataServiceClient(channel);

                return await client.GetStudentsWithEligibleCoursesAsync(new GetStudentsRequest());
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while calling external service: {ex.Message}");

            return new GetStudentsResponse
            {
                Success = false,
                Message = $"Failed to connect to external service: {ex.Message}"
            };

        }
    }
}