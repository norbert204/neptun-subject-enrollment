using NeptunKiller.UserService.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ServiceOptions>(builder.Configuration.GetSection("Services"));
var serviceOptions = builder.Configuration.GetSection("Services").Get<ServiceOptions>();

builder.Services.AddGrpc();
builder.Services.AddSingleton<NeptunKiller.UserService.Functions.UserService>();

builder.Services.AddGrpcClient<GrpcDatabaseService.Protos.UserService.UserServiceClient>(x => x.Address = serviceOptions.DatabaseServiceUri)
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

app.MapGrpcService<NeptunKiller.UserService.Functions.UserService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();