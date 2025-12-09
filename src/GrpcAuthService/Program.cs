using GrpcCachingService;
using GrpcDatabaseService.Protos;
using GrpcAuthService.Options;
using GrpcAuthService.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Force HTTP/2 on port 80 for gRPC over h2c
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

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

app.MapGrpcService<AuthService>();

app.Run();
