using System.Net;
using System.Net.Http;
using System.Net.Security;
using Gateway.Helpers;
using Gateway.Options;
using GrpcAuthService;
using GrpcDatabaseService.Protos;
using Serilog;
using SubjectService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddSerilog(x => x
    .MinimumLevel.Information()
    .WriteTo.Console());

var serviceLocationOptions = builder.Configuration.GetSection("ServiceLocation").Get<ServiceLocationOptions>();

builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(x => x.Address = new Uri(serviceLocationOptions!.AuthServiceUri))
    .ConfigurePrimaryHttpMessageHandler(CreateH2cHandler);

builder.Services.AddGrpcClient<Subject.SubjectClient>(x => x.Address = new Uri(serviceLocationOptions!.SubjectServiceUri))
    .ConfigurePrimaryHttpMessageHandler(CreateH2cHandler);

builder.Services.AddGrpcClient<CourseService.CourseServiceClient>(x => x.Address = new Uri(serviceLocationOptions.DatabaseServiceUri))
    .ConfigurePrimaryHttpMessageHandler(CreateH2cHandler);

builder.Services.AddGrpcClient<GrpcDatabaseService.Protos.SubjectService.SubjectServiceClient>(x => x.Address = new Uri(serviceLocationOptions.DatabaseServiceUri))
    .ConfigurePrimaryHttpMessageHandler(CreateH2cHandler);

builder.Services.AddGrpcClient<UserService.UserServiceClient>(x => x.Address = new Uri(serviceLocationOptions.DatabaseServiceUri))
    .ConfigurePrimaryHttpMessageHandler(CreateH2cHandler);

var app = builder.Build();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disabled CORS for less suffering
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();

static HttpMessageHandler CreateH2cHandler()
{
    return new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.None,
        UseProxy = false,
        EnableMultipleHttp2Connections = true,
        Http2Only = true,
        SslOptions = new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = (_, _, _, _) => true
        }
    };
}