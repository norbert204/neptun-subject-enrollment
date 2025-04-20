using GrpcCachingService.Repositories;
using GrpcCachingService.Repositories.Interfaces;
using GrpcCachingService.Services;
using GrpcCachingService.Services.Interfaces;
using StackExchange.Redis;

namespace GrpcCachingService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddGrpc();

        var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        builder.Services.Configure<CourseDataServiceOptions>(builder.Configuration.GetSection("CourseDataService"));


        builder.Services.AddSingleton<IRedisRepository, RedisRepository>();
        builder.Services.AddSingleton<ICourseRegistrationRepository, CourseRegistrationRepository>();
        builder.Services.AddSingleton<ICourseDataServiceClient, CourseDataServiceClient>();
        builder.Services.AddSingleton<ICourseInitializerService, CourseInitializerService>();

        var app = builder.Build();

        app.MapGrpcService<RedisServiceImpl>();
        app.MapGrpcService<CourseRegistrationServiceImpl>();

        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }
}