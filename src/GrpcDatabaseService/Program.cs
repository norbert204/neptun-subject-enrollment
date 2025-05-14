using GrpcDatabaseService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddSingleton<DatabaseContext>(provider =>
{
    // Get the content root path
    var contentRootPath = builder.Environment.ContentRootPath;
    // Create a data directory path
    var dataDirectory = Path.Combine(contentRootPath, "Data");

    return new DatabaseContext(dataDirectory);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//app.MapGrpcService<GreeterService>();
app.MapGrpcService<UserService>();
app.MapGrpcService<CourseService>();
app.MapGrpcService<SubjectService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
