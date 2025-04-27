using GrpcCachingService;
using NeptunKiller.SubjectService.Options;
using NeptunKiller.SubjectService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddOptions<ServiceOptions>("Services");

var serviceOptions = builder.Configuration.GetSection("Services").Get<ServiceOptions>();

builder.Services.AddGrpcClient<CourseRegistrationService.CourseRegistrationServiceClient>(
    x => x.Address = serviceOptions.CachingServiceUri);

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