using GrpcCachingService;
using GrpcDatabaseService.Protos;
using NeptunKiller.SubjectService.Options;
using NeptunKiller.SubjectService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.Configure<ServiceOptions>(builder.Configuration.GetSection("Services"));

var serviceOptions = builder.Configuration.GetSection("Services").Get<ServiceOptions>();

builder.Services.AddGrpcClient<CourseRegistrationService.CourseRegistrationServiceClient>(x => x.Address = serviceOptions.CachingServiceUri)
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return handler;
    });

builder.Services.AddGrpcClient<UserService.UserServiceClient>(x => x.Address = serviceOptions.DatabaseServiceUri)
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return handler;
    });

builder.Services.AddGrpcClient<GrpcDatabaseService.Protos.SubjectService.SubjectServiceClient>(x => x.Address = serviceOptions.DatabaseServiceUri)
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return handler;
    });

builder.Services.AddGrpcClient<CourseService.CourseServiceClient>(x => x.Address = serviceOptions.DatabaseServiceUri)
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return handler;
    });

builder.Services.AddSingleton<ICacheService, CacheService>();

var app = builder.Build();

app.MapGrpcService<NeptunKiller.SubjectService.Functions.SubjectService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();