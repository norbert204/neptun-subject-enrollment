using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcCachingService;
using GrpcDatabaseService.Protos;
using NeptunKiller.SubjectService.Exceptions;
using NeptunKiller.SubjectService.Services;
using SubjectService;

namespace NeptunKiller.SubjectService.Functions;

public class SubjectService : Subject.SubjectBase
{
    private readonly ILogger<SubjectService> _logger;
    private readonly ICacheService _cacheService;
    private readonly GrpcDatabaseService.Protos.SubjectService.SubjectServiceClient _databaseSubjectService;
    private readonly UserService.UserServiceClient _databaseUserService;
    private readonly CourseService.CourseServiceClient _databaseCourseService;
    private readonly CourseRegistrationService.CourseRegistrationServiceClient _courseRegistrationServiceClient;

    public SubjectService(
        ILogger<SubjectService> logger,
        ICacheService cacheService,
        GrpcDatabaseService.Protos.SubjectService.SubjectServiceClient databaseSubjectService,
        UserService.UserServiceClient databaseUserService,
        CourseRegistrationService.CourseRegistrationServiceClient courseRegistrationServiceClient,
        CourseService.CourseServiceClient databaseCourseService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _databaseSubjectService = databaseSubjectService;
        _databaseUserService = databaseUserService;
        _courseRegistrationServiceClient = courseRegistrationServiceClient;
        _databaseCourseService = databaseCourseService;
    }

    public override async Task<EnrollToCourseResponse> EnrollToCourse(EnrollToCourseRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Enrolling student {StudentId} to course {CourseId}", request.StudentId, request.CourseId);

        try
        {
            if (await _cacheService.IsCourseFullAsync(request.CourseId))
            {
                return new EnrollToCourseResponse
                {
                    Success = false,
                    Message = "Course is full",
                };
            }

            if (!await _cacheService.CanStudentEnrollToCourseAsync(request.StudentId, request.CourseId))
            {
                return new EnrollToCourseResponse
                {
                    Success = false,
                    Message = "Student is not allowed to enroll to this course",
                };
            }

            await _cacheService.EnrollToCourseAsync(request.CourseId, request.StudentId);

            return new EnrollToCourseResponse
            {
                Success = true,
                Message = "Successful enrollment",
            };
        }
        catch (CacheServiceException ex)
        {
            return new EnrollToCourseResponse
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }

    public override async Task<EnrollmentInitializationResponse> InitializeSubjectEnrollment(Empty request, ServerCallContext context)
    {
        var students = await _databaseUserService.ListUsersAsync(new GetAllUsersRequest());
        var subjects = await _databaseSubjectService.ListSubjectsAsync(new GetAllSubjectsRequest());

        await Parallel.ForEachAsync(subjects.Subjects.ToList(), async (subject, _) =>
        {
            foreach (var course in subject.Courses)
            {
                var courseData = await _databaseCourseService.GetCourseAsync(
                    new CourseIdRequest
                    {
                        Id = course,
                    });

                await _courseRegistrationServiceClient.InitializeCourseAsync(
                    new InitializeCourseRequest
                    {
                        CourseId = course,
                        MaxStudents = courseData.Course.Capacity,
                    });
            }
        });

        await Parallel.ForEachAsync(students.Users, async (student, _) =>
        {
            // TODO: Only register courses that are truly eligible for students
            var courses = subjects.Subjects.SelectMany(x => x.Courses);

            await _courseRegistrationServiceClient.InitializeStudentAsync(
                new InitializeStudentRequest
                {
                    NeptunCode = student.NeptunCode,
                    CourseId = { courses },
                });
        });

        return new EnrollmentInitializationResponse
        {
            Success = true,
            Message = "Successful enrollment initialization",
        };
    }
}