using Polly.Retry;
using Polly;
using StackExchange.Redis;
using System.Net.Sockets;
using GrpcCachingService.Repositories.Interfaces;
using System.Reflection;

namespace GrpcCachingService.Repositories;

public class CourseRegistrationRepository : ICourseRegistrationRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _cache;
    private readonly ILogger<CourseRegistrationRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private const int DefaultMaxEnrollment = 999;

    private readonly string _registerScript;
    private readonly string _unregisterScript;

    public CourseRegistrationRepository(IConnectionMultiplexer redis, ILogger<CourseRegistrationRepository> logger)
    {
        _redis = redis;
        _cache = redis.GetDatabase();
        _logger = logger;

        _registerScript = LoadScriptFromResource("RegisterCourse.lua");
        _unregisterScript = LoadScriptFromResource("UnregisterCourse.lua");

        _retryPolicy = Policy
            .Handle<RedisConnectionException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) =>
                {
                    _logger.LogWarning($"Redis operation failed. Retrying in {time.TotalSeconds} seconds. Error: {ex.Message}");
                });
    }
    public async Task<(bool Success, string Message)> RegisterStudentForCourseAsync(string courseCode, string studentId)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                await InitializeCourseAsync(courseCode);

                var courseKey = $"course:{courseCode}:students";
                var studentKey = $"student:{studentId}:courses";
                var enrollmentCountKey = $"course:{courseCode}:currentEnrollment";
                var maxEnrollmentKey = $"course:{courseCode}:maxEnrollment";

                // Lua szkript végrehajtása a tárgyleadáshoz
                // Lua szkript a redisben egymás után hajtódik végre tehát nem kell foglalkozni a sorral
                var result = await _cache.ScriptEvaluateAsync(
                    _registerScript,
                    new RedisKey[] { courseKey, enrollmentCountKey, maxEnrollmentKey },
                    new RedisValue[] { studentId }
                );

                var resultArray = (RedisValue[]?)result;
                var success = (int)resultArray![0] == 1;
                var status = resultArray[1].ToString();

                if (success)
                {
                    await _cache.SetAddAsync(studentKey, courseCode);
                    return (true, $"Student with ID {studentId} successfully registered for course {courseCode}.");
                }
                else
                {
                    switch (status)
                    {
                        case "already_registered":
                            return (false, $"Student with ID {studentId} has already registered for course {courseCode}.");
                        case "course_full":
                            var status2 = await GetCourseEnrollmentStatusAsync(courseCode);
                            return (false, $"Course {courseCode} is already full ({status2.CurrentEnrollment}/{status2.MaxEnrollment}).");
                        default:
                            return (false, "An unknown error occurred during course registration.");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in{nameof(RegisterStudentForCourseAsync)}: {ex.Message}");
            return (false, "A system error occurred during course registration.");
        }
    }

    public async Task<bool> UnregisterStudentFromCourseAsync(string courseCode, string studentId)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var courseKey = $"course:{courseCode}:students";
                var studentKey = $"student:{studentId}:courses";
                var enrollmentCountKey = $"course:{courseCode}:currentEnrollment";

                var result = await _cache.ScriptEvaluateAsync(
                    _unregisterScript,
                    new RedisKey[] { courseKey, enrollmentCountKey },
                    new RedisValue[] { studentId }
                );

                var success = (int)result == 1;

                if (success)
                {
                    await _cache.SetRemoveAsync(studentKey, courseCode);
                }

                return success;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in{nameof(UnregisterStudentFromCourseAsync)}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> InitializeCourseAsync(string courseCode, int maxEnrollment = DefaultMaxEnrollment)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var maxEnrollmentKey = $"course:{courseCode}:maxEnrollment";
                var currentEnrollmentKey = $"course:{courseCode}:currentEnrollment";

                var maxEnrollmentExists = await _cache.KeyExistsAsync(maxEnrollmentKey);
                var currentEnrollmentExists = await _cache.KeyExistsAsync(currentEnrollmentKey);

                var batch = _cache.CreateBatch();

                if (!maxEnrollmentExists)
                {
                    _ = batch.StringSetAsync(maxEnrollmentKey, maxEnrollment);
                }

                if (!currentEnrollmentExists)
                {
                    _ = batch.StringSetAsync(currentEnrollmentKey, 0);
                }

                batch.Execute();

                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in{nameof(InitializeCourseAsync)}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> InitializeStudentEligibleCoursesAsync(string studentId, IEnumerable<string> eligibleCourseCodes)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var eligibleCoursesKey = $"student:{studentId}:eligibleCourses";

                await _cache.KeyDeleteAsync(eligibleCoursesKey);

                if (!eligibleCourseCodes.Any())
                {
                    return true;
                }

                var values = eligibleCourseCodes.Select(code => (RedisValue)code).ToArray();
                await _cache.SetAddAsync(eligibleCoursesKey, values);

                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in {nameof(InitializeStudentEligibleCoursesAsync)}: {ex.Message}");
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetStudentsForCourseAsync(string courseCode)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var courseKey = $"course:{courseCode}:students";
                var members = await _cache.SetMembersAsync(courseKey);
                return members.Select(m => m.ToString());
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in {nameof(GetStudentsForCourseAsync)}: {ex.Message}");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IEnumerable<string>> GetCoursesForStudentAsync(string studentId)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var studentKey = $"student:{studentId}:courses";
                var members = await _cache.SetMembersAsync(studentKey);
                return members.Select(m => m.ToString());
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in {nameof(GetCoursesForStudentAsync)}: {ex.Message}");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<(int CurrentEnrollment, int MaxEnrollment, bool IsFull)> GetCourseEnrollmentStatusAsync(string courseCode)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                await InitializeCourseAsync(courseCode);

                var maxEnrollmentKey = $"course:{courseCode}:maxEnrollment";
                var currentEnrollmentKey = $"course:{courseCode}:currentEnrollment";

                var batch = _cache.CreateBatch();

                var maxTask = batch.StringGetAsync(maxEnrollmentKey);
                var currentTask = batch.StringGetAsync(currentEnrollmentKey);

                batch.Execute();

                await Task.WhenAll(maxTask, currentTask);

                var maxEnrollment = maxTask.Result.HasValue ? int.Parse(maxTask.Result!) : DefaultMaxEnrollment;
                var currentEnrollment = currentTask.Result.HasValue ? int.Parse(currentTask.Result!) : 0;

                return (currentEnrollment, maxEnrollment, currentEnrollment >= maxEnrollment);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in{nameof(GetCourseEnrollmentStatusAsync)}: {ex.Message}");
            return (0, DefaultMaxEnrollment, false);
        }
    }

    
    public async Task<IEnumerable<string>> GetEligibleCoursesForStudentAsync(string studentId)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var eligibleCoursesKey = $"student:{studentId}:eligibleCourses";
                var members = await _cache.SetMembersAsync(eligibleCoursesKey);
                return members.Select(m => m.ToString());
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in{nameof(GetEligibleCoursesForStudentAsync)}: {ex.Message}");
            return Enumerable.Empty<string>();
        }
    }

    private string LoadScriptFromResource(string scriptName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(scriptName));

        if (resourceName == null)
        {
            _logger.LogError($"Script resource {scriptName} not found");
            throw new FileNotFoundException($"Script resource {scriptName} not found");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public async Task<IEnumerable<(string CourseCode, IEnumerable<string> StudentIds)>> GetAllCoursesWithStudentsAsync()
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var result = new List<(string CourseCode, IEnumerable<string> StudentIds)>();

                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var courseKeys = server.Keys(pattern: "course:*:students").ToArray();

                foreach (var key in courseKeys)
                {
                    var courseCode = key.ToString().Split(':')[1];
                    var students = await _cache.SetMembersAsync(key);

                    result.Add((courseCode, students.Select(s => s.ToString())));
                }

                return result;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in {nameof(GetAllCoursesWithStudentsAsync)} operation: {ex.Message}");
            return Enumerable.Empty<(string, IEnumerable<string>)>();
        }
    }

    public async Task<IEnumerable<(string StudentId, IEnumerable<string> EligibleCourses)>> GetAllStudentsWithCoursesAsync()
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var result = new List<(string StudentId, IEnumerable<string> EligibleCourses)>();

                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var studentKeys = server.Keys(pattern: "student:*:courses").ToArray();

                foreach (var key in studentKeys)
                {
                    var studentId = key.ToString().Split(':')[1];
                    var eligibleCoursesKey = $"student:{studentId}:eligibleCourses";
                    var eligibleCourses = await _cache.SetMembersAsync(eligibleCoursesKey);

                    result.Add((
                        studentId,
                        eligibleCourses.Select(c => c.ToString())
                    ));
                }

                return result;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in {nameof(GetAllStudentsWithCoursesAsync)} operation: {ex.Message}");
            return Enumerable.Empty<(string, IEnumerable<string>)>();
        }
    }
}