using CourseRegistrationService.External;
using GrpcCachingService.Repositories.Interfaces;
using GrpcCachingService.Services.Interfaces;

namespace GrpcCachingService.Services;

public class CourseInitializerService : ICourseInitializerService
{
    private readonly ICourseDataServiceClient _courseDataClient;
    private readonly ICourseRegistrationRepository _repository;
    private readonly ILogger<CourseInitializerService> _logger;

    public CourseInitializerService(
        ICourseDataServiceClient courseDataClient,
        ICourseRegistrationRepository repository,
        ILogger<CourseInitializerService> logger)
    {
        _courseDataClient = courseDataClient;
        _repository = repository;
        _logger = logger;
    }

    public async Task<(int InitializedCount, bool Success, string Message)> InitializeCoursesAsync(bool forceUpdate = false)
    {
        try
        {
            var response = await _courseDataClient.GetAllCoursesAsync();

            if (!response.Success)
            {
                _logger.LogWarning($"External service returned an error: {response.Message}");

                // ha nem működik a service, akkor teszt adatokkal megyünk tovább
                // ez csak teszteléshez kell később ki kell venni
                response = GetTestCourseData();
            }

            int initializedCount = 0;

            foreach (var course in response.Courses)
            {
                bool initialized = await _repository.InitializeCourseAsync(course.CourseCode, course.MaxEnrollment);

                if (initialized)
                {
                    initializedCount++;
                    _logger.LogInformation($"Course initialized: {course.CourseCode}, capacity: {course.MaxEnrollment}");
                }
            }

            return (initializedCount, true, $"{initializedCount} course(s) successfully initialized.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while initializing courses: {ex.Message}");
            return (0, false, $"An error occurred while initializing courses: {ex.Message}");
        }
    }

    public async Task<(int InitializedCount, bool Success, string Message)> InitializeStudentsAsync(bool forceUpdate = false)
    {
        try
        {
            var response = await _courseDataClient.GetStudentsWithEligibleCoursesAsync();

            if (!response.Success)
            {
                _logger.LogWarning($"External service returned an error while fetching students: {response.Message}");

                // ha nem működik a service, akkor teszt adatokkal megyünk tovább
                // ez csak teszteléshez kell később ki kell venni
                response = GetTestStudentData();
            }

            int initializedCount = 0;

            foreach (var student in response.Students)
            {
                bool initialized = await _repository.InitializeStudentEligibleCoursesAsync(
                    student.StudentId,
                    student.EligibleCourseCodes);

                if (initialized)
                {
                    initializedCount++;
                    _logger.LogInformation($"Student initialized: {student.StudentId}, number of eligible courses: {student.EligibleCourseCodes.Count}");
                }
            }

            return (initializedCount, true, $"{initializedCount} student(s) successfully initialized.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while initializing students: {ex.Message}");
            return (0, false, $"An error occurred while initializing students: {ex.Message}");
        }
    }

    public async Task<(int InitializedCourseCount, int InitializedStudentCount, bool Success, string Message)> InitializeAllAsync(bool forceUpdate = false)
    {
        try
        {
            var (courseCount, courseSuccess, courseMessage) = await InitializeCoursesAsync(forceUpdate);

            var (studentCount, studentSuccess, studentMessage) = await InitializeStudentsAsync(forceUpdate);

            bool success = courseSuccess && studentSuccess;
            string message = $"Initialization result: {courseMessage} {studentMessage}";

            return (courseCount, studentCount, success, message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while initializing data: {ex.Message}");
            return (0, 0, false, $"An error occurred while initializing data: {ex.Message}");
        }
    }

    private GetAllCoursesResponse GetTestCourseData()
    {
        return new GetAllCoursesResponse
        {
            Success = true,
            Message = "Teszt adatok betöltve",
            Courses =
            {
                new CourseData { CourseCode = "IK-PROG1001", CourseName = "Programozás alapjai", MaxEnrollment = 10 },
                new CourseData { CourseCode = "IK-PROG1002", CourseName = "Objektumorientált programozás", MaxEnrollment = 8 },
                new CourseData { CourseCode = "IK-ALG1001", CourseName = "Algoritmusok és adatszerkezetek", MaxEnrollment = 12 },
                new CourseData { CourseCode = "IK-WEB1001", CourseName = "Webfejlesztés", MaxEnrollment = 15 },
                new CourseData { CourseCode = "IK-DB1001", CourseName = "Adatbázisok", MaxEnrollment = 20 },
                new CourseData { CourseCode = "IK-MATH1001", CourseName = "Diszkrét matematika", MaxEnrollment = 30 },
                new CourseData { CourseCode = "IK-NET1001", CourseName = "Számítógép hálózatok", MaxEnrollment = 25 },
                new CourseData { CourseCode = "IK-OS1001", CourseName = "Operációs rendszerek", MaxEnrollment = 18 },
                new CourseData { CourseCode = "IK-AI1001", CourseName = "Mesterséges intelligencia", MaxEnrollment = 10 },
                new CourseData { CourseCode = "IK-SEC1001", CourseName = "Informatikai biztonság", MaxEnrollment = 15 }
            }
        };
    }

    private GetStudentsResponse GetTestStudentData()
    {
        return new GetStudentsResponse
        {
            Success = true,
            Message = "Teszt diák adatok betöltve",
            Students =
            {
                new StudentEligibleCourses
                {
                    StudentId = "NEPTUN001",
                    EligibleCourseCodes = { "IK-PROG1001", "IK-ALG1001", "IK-WEB1001", "IK-DB1001" }
                },
                new StudentEligibleCourses
                {
                    StudentId = "NEPTUN002",
                    EligibleCourseCodes = { "IK-PROG1002", "IK-MATH1001", "IK-NET1001" }
                },
                new StudentEligibleCourses
                {
                    StudentId = "NEPTUN003",
                    EligibleCourseCodes = { "IK-OS1001", "IK-AI1001", "IK-SEC1001", "IK-PROG1001" }
                },
                new StudentEligibleCourses
                {
                    StudentId = "NEPTUN004",
                    EligibleCourseCodes = { "IK-PROG1001", "IK-PROG1002", "IK-DB1001", "IK-WEB1001" }
                },
                new StudentEligibleCourses
                {
                    StudentId = "NEPTUN005",
                    EligibleCourseCodes = { "IK-MATH1001", "IK-ALG1001", "IK-AI1001" }
                },
                new StudentEligibleCourses
                {
                    StudentId = "NEPTUN006",
                    EligibleCourseCodes = { "IK-SEC1001", "IK-NET1001", "IK-OS1001" }
                },
                new StudentEligibleCourses
                {
                    StudentId = "NEPTUN007",
                    EligibleCourseCodes = { "IK-PROG1001", "IK-PROG1002", "IK-ALG1001" }
                },
                new StudentEligibleCourses
                {
                    StudentId = "NEPTUN008",
                    EligibleCourseCodes = { "IK-WEB1001", "IK-DB1001", "IK-AI1001" }
                },
                new StudentEligibleCourses
                {
                    StudentId = "NEPTUN009",
                    EligibleCourseCodes = { "IK-MATH1001", "IK-NET1001", "IK-SEC1001" }
                },
                new StudentEligibleCourses
                {
                    StudentId = "NEPTUN010",
                    EligibleCourseCodes = { "IK-OS1001", "IK-PROG1001", "IK-DB1001" }
                }
            }
        };
    }
}