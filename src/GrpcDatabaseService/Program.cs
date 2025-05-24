using GrpcDatabaseService.Repositories;
using GrpcDatabaseService.Repositories.Interfaces;
using GrpcDatabaseService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// Configure Cassandra connection
builder.Services.AddSingleton(provider =>
    new CassandraConnection(
        builder.Configuration.GetValue<string>("CassandraSettings:ContactPoints"),
        builder.Configuration.GetValue<string>("CassandraSettings:Keyspace")
    ));

// Register repositories
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<UserService>();
app.MapGrpcService<CourseService>();
app.MapGrpcService<SubjectService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
