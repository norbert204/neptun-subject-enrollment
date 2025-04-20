using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using GrpcCachingService.Repositories.Interfaces;
using GrpcCachingService.Repositories;

namespace CourseRegistration.Tests;

public class CourseRegistrationIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICourseRegistrationRepository _repository;
    private readonly IConnectionMultiplexer _redis;

    public CourseRegistrationIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        var services = new ServiceCollection();

        var redisConnectionString = "localhost:6379";
        _redis = ConnectionMultiplexer.Connect(redisConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(_redis);

        services.AddLogging(builder =>
        {
            builder.AddProvider(new XUnitLoggerProvider(_output));
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        services.AddSingleton<ICourseRegistrationRepository, CourseRegistrationRepository>();

        _serviceProvider = services.BuildServiceProvider();

        _repository = _serviceProvider.GetRequiredService<ICourseRegistrationRepository>();
    }

    public void Dispose()
    {
        _redis.Dispose();
    }

    [Fact]
    public async Task TestParallelRegistration()
    {
        string courseCode = "IK-PROG1234";
        int maxEnrollment = 10;

        await _repository.InitializeCourseAsync(courseCode, maxEnrollment);

        for (int i = 1; i <= 15; i++)
        {
            var studentId = $"NEPTUN{i:D3}";
            await _repository.InitializeStudentEligibleCoursesAsync(studentId, new[] { courseCode });
        }

        // Párhuzamos tárgyfelvétel tesztelése
        var tasks = new List<Task<(bool Success, string Message)>>();

        // 15 diák próbál egyszerre felvenni egy 10 fős tárgyat
        for (int i = 1; i <= 15; i++)
        {
            var studentId = $"NEPTUN{i:D3}";
            tasks.Add(_repository.RegisterStudentForCourseAsync(courseCode, studentId));
        }

        // Megvárjuk az összes kérés befejezését
        var results = await Task.WhenAll(tasks);

        int successCount = results.Count(r => r.Success);
        int failCount = results.Count(r => !r.Success);

        _output.WriteLine($"Sikeres tárgyfelvételek: {successCount}");
        _output.WriteLine($"Sikertelen tárgyfelvételek: {failCount}");

        // Ellenőrizzük a tárgy létszámát
        var status = await _repository.GetCourseEnrollmentStatusAsync(courseCode);

        _output.WriteLine($"Tárgy létszáma: {status.CurrentEnrollment}/{status.MaxEnrollment}");

        // Ellenőrizzük, hogy pontosan 10 sikeres tárgyfelvétel történt
        Assert.Equal(maxEnrollment, successCount);
        Assert.Equal(5, failCount);
        Assert.Equal(maxEnrollment, status.CurrentEnrollment);
        Assert.True(status.IsFull);
    }
}

public class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _testOutputHelper;

    public XUnitLoggerProvider(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName)
        => new XUnitLogger(_testOutputHelper, categoryName);

    public void Dispose() { }
}

public class XUnitLogger : ILogger
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _categoryName;

    public XUnitLogger(ITestOutputHelper testOutputHelper, string categoryName)
    {
        _testOutputHelper = testOutputHelper;
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        try
        {
            _testOutputHelper.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} [{logLevel}] {_categoryName}: {formatter(state, exception)}");
            if (exception != null)
                _testOutputHelper.WriteLine(exception.ToString());
        }
        catch (InvalidOperationException)
        {
            // A teszt már befejeződött, nem lehet írni a kimenetére
        }
    }

    private class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance = new NoopDisposable();
        public void Dispose() { }
    }
}
