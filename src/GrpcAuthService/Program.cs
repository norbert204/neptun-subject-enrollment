using GrpcCachingService;
using GrpcDatabaseService.Protos;
using GrpcAuthService.Options;
using GrpcAuthService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var serviceOptions = builder.Configuration.GetSection("Service").Get<ServiceOptions>();

builder.Services.AddGrpc();

builder.Services.AddGrpcClient<AuthDataService.AuthDataServiceClient>(x => x.Address = new Uri(serviceOptions.CacheServiceUri))
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return handler;
    });

builder.Services.AddGrpcClient<UserService.UserServiceClient>(x => x.Address = new Uri(serviceOptions.DatabaseServiceUri))
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return handler;
    });

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGrpcService<AuthService>();

app.Run();
