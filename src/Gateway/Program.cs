using Gateway.Options;
using GrpcAuthService;
using SubjectService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var serviceLocationOptions = builder.Configuration.GetSection("ServiceLocation").Get<ServiceLocationOptions>();

builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(x => x.Address = new Uri(serviceLocationOptions!.AuthServiceUri))
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return handler;
    });

builder.Services.AddGrpcClient<Subject.SubjectClient>(x => x.Address = new Uri(serviceLocationOptions!.SubjectServiceUri))
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return handler;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();