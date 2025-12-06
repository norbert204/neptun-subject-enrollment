using Gateway.Options;
using GrpcAuthService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var serviceLocationOptions = builder.Configuration.GetSection("ServiceLocation").Get<ServiceLocationOptions>();

builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(x => x.Address = new Uri(serviceLocationOptions!.AuthServiceUri));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();