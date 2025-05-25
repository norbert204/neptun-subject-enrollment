using CourseRegistrationService;
using CourseRegistrationService.External;
using Grpc.Core;
using GrpcCachingService.Repositories.Interfaces;
using GrpcCachingService.Services.Interfaces;
using Microsoft.AspNetCore.Identity.Data;

namespace GrpcCachingService.Services;

public class CourseRegistrationServiceImpl : CourseRegistrationService.CourseRegistrationServiceBase
{
    private readonly ICourseRegistrationRepository _repository;
    private readonly ILogger<CourseRegistrationServiceImpl> _logger;
    private readonly ICourseInitializerService _initializerService;

    public CourseRegistrationServiceImpl(ICourseRegistrationRepository repository, ILogger<CourseRegistrationServiceImpl> logger, ICourseInitializerService initializerService)
    {
        _repository = repository;
        _logger = logger;
        _initializerService = initializerService;
    }

    public override async Task<RegisterResponse> RegisterStudentForCourse(RegisterRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Registered: {request.StudentId} - {request.CourseCode}");

        if (string.IsNullOrEmpty(request.CourseCode) || string.IsNullOrEmpty(request.StudentId))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "The courseId and the neptun code cannot be empty!"
            };
        }

        (var success, var message) = await _repository.RegisterStudentForCourseAsync(request.CourseCode, request.StudentId);

        return new RegisterResponse
        {
            Success = success,
            Message = message
        };
    }

    public override async Task<UnregisterResponse> UnregisterStudentFromCourse(UnregisterRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Student is unregistering from course: {request.StudentId} - {request.CourseCode}");

        if (string.IsNullOrEmpty(request.CourseCode) || string.IsNullOrEmpty(request.StudentId))
        {
            return new UnregisterResponse
            {
                Success = false,
                Message = "The courseId and the neptun code cannot be empty!"
            };
        }

        var success = await _repository.UnregisterStudentFromCourseAsync(request.CourseCode, request.StudentId);

        return new UnregisterResponse
        {
            Success = success,
            Message = success
                ? $"Student with ID {request.StudentId} successfully unregistered from course {request.CourseCode}."
                : "An error occurred while unregistering from the course."

        };
    }

    public override async Task<StudentsResponse> GetStudentsForCourse(CourseRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Fetching students for course: {request.CourseCode}");

        if (string.IsNullOrEmpty(request.CourseCode))
        {
            return new StudentsResponse
            {
                Success = false,
                Message = "The course code cannot be empty!"
            };
        }

        var students = await _repository.GetStudentsForCourseAsync(request.CourseCode);

        var response = new StudentsResponse
        {
            Success = true,
            Message = $"List of students for course {request.CourseCode} successfully retrieved."
        };

        response.StudentIds.AddRange(students);

        return response;
    }

    public override async Task<CoursesResponse> GetCoursesForStudent(StudentRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Fetching courses for student: {request.StudentId}");

        if (string.IsNullOrEmpty(request.StudentId))
        {
            return new CoursesResponse
            {
                Success = false,
                Message = "The neptun code cannot be empty!"
            };
        }

        var courses = await _repository.GetCoursesForStudentAsync(request.StudentId);

        var response = new CoursesResponse
        {
            Success = true,
            Message = $"List of courses for student with ID {request.StudentId} successfully retrieved."
        };

        response.CourseCodes.AddRange(courses);

        return response;
    }


    public override async Task<InitializeCoursesResponse> InitializeCourses(InitializeCoursesRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Course initialization request received. Force update: {request.ForceUpdate}");

        var (initializedCount, success, message) = await _initializerService.InitializeCoursesAsync(request.ForceUpdate);

        return new InitializeCoursesResponse
        {
            InitializedCoursesCount = initializedCount,
            Success = success,
            Message = message
        };
    }

    public override async Task<InitializeStudentsResponse> InitializeStudents(InitializeStudentsRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Student initialization request received. Force update: {request.ForceUpdate}");

        var (initializedCount, success, message) = await _initializerService.InitializeStudentsAsync(request.ForceUpdate);

        return new InitializeStudentsResponse
        {
            InitializedStudentsCount = initializedCount,
            Success = success,
            Message = message
        };
    }

    public override async Task<InitializeAllResponse> InitializeAll(InitializeAllRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Initialize all data request received. Force update: {request.ForceUpdate}");

        var (courseCount, studentCount, success, message) =
            await _initializerService.InitializeAllAsync(request.ForceUpdate);

        return new InitializeAllResponse
        {
            InitializedCoursesCount = courseCount,
            InitializedStudentsCount = studentCount,
            Success = success,
            Message = message
        };
    }

    public override async Task<EligibleCoursesResponse> GetEligibleCoursesForStudent(StudentRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Fetching eligible courses for student: {request.StudentId}");

        if (string.IsNullOrEmpty(request.StudentId))
        {
            return new EligibleCoursesResponse
            {
                Success = false,
                Message = "The neptun code cannot be empty!"
            };
        }

        var courses = await _repository.GetEligibleCoursesForStudentAsync(request.StudentId);

        var response = new EligibleCoursesResponse
        {
            Success = true,
            Message = $"List of eligible courses for student with ID {request.StudentId} successfully retrieved."
        };

        response.CourseCodes.AddRange(courses);

        return response;
    }

    public override async Task<AllCoursesWithStudentsResponse> GetAllCoursesWithStudents(GetAllCoursesRequest2 request, ServerCallContext context)
    {
        _logger.LogInformation("Querying all courses and their students");

        var coursesWithStudents = await _repository.GetAllCoursesWithStudentsAsync();

        var response = new AllCoursesWithStudentsResponse
        {
            Success = true,
            Message = "Courses and their students successfully retrieved"
        };

        foreach (var (courseCode, studentIds) in coursesWithStudents)
        {
            var status = await _repository.GetCourseEnrollmentStatusAsync(courseCode);

            var courseWithStudents = new CourseWithStudents
            {
                CourseCode = courseCode,
                CourseName = courseCode, // lehet kell a tárgy neve is?
                CurrentEnrollment = status.CurrentEnrollment,
                MaxEnrollment = status.MaxEnrollment
            };

            courseWithStudents.StudentIds.AddRange(studentIds);

            response.Courses.Add(courseWithStudents);
        }

        return response;
    }

    public override async Task<AllStudentsWithEligibleCoursesResponse> GetAllStudentsWithEligibleCourses(GetAllStudentsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Querying all students and their eligible courses");

        var studentsWithCourses = await _repository.GetAllStudentsWithCoursesAsync();

        var response = new AllStudentsWithEligibleCoursesResponse
        {
            Success = true,
            Message = "Students and their eligible courses successfully retrieved"
        };

        foreach (var (studentId, eligibleCourses) in studentsWithCourses)
        {
            var studentWithCourses = new StudentWithCourses
            {
                StudentId = studentId
            };

            studentWithCourses.EligibleCourseCodes.AddRange(eligibleCourses);

            response.Students.Add(studentWithCourses);
        }

        return response;
    }

    public override async Task<InitializeStudentResponse> InitializeStudent(InitializeStudentRequest request, ServerCallContext context)
    {
        var result = await _repository.InitializeStudentEligibleCoursesAsync(request.NeptunCode, request.CourseId);

        if (!result)
        {
            return new InitializeStudentResponse
            {
                Success = false,
                Message = $"Failed to initialize student with ID {request.NeptunCode}",
            };
        }
        
        return new InitializeStudentResponse
        {
            Success = true,
            Message = $"Succesfully initialized student with ID {request.NeptunCode}",
        };
    }

    public override async Task<InitializeCourseResponse> InitializeCourse(InitializeCourseRequest request, ServerCallContext context)
    {
        var result = await _repository.InitializeCourseAsync(request.CourseId, request.MaxStudents);

        if (!result)
        {
            return new InitializeCourseResponse
            {
                Success = false,
                Message = $"Failed to initialize course with ID {request.CourseId}",
            };
        }

        return new InitializeCourseResponse
        {
            Success = true,
            Message = $"Succesfully initialized course with ID {request.CourseId}",
        };
    }
}