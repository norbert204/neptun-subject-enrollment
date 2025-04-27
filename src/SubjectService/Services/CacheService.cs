using GrpcCachingService;
using NeptunKiller.SubjectService.Exceptions;

namespace NeptunKiller.SubjectService.Services;

public class CacheService : ICacheService
{
    private readonly CourseRegistrationService.CourseRegistrationServiceClient _courseRegistrationServiceClient;
    
    public CacheService(CourseRegistrationService.CourseRegistrationServiceClient courseRegistrationServiceClient)
    {
        _courseRegistrationServiceClient = courseRegistrationServiceClient;
    }

    public async Task<bool> CanStudentEnrollToCourse(string courseId, string studentId)
    {
        var request = new StudentRequest
        {
            StudentId = studentId,
        };

        var result = await _courseRegistrationServiceClient.GetEligibleCoursesForStudentAsync(request);

        if (!result.Success)
        {
            throw new CacheServiceException(result.Message);
        }
        
        return result.CourseCodes.Contains(courseId);
    }

    public async Task<bool> IsCourseFull(string courseId)
    {
        var request = new CourseRequest
        {
            CourseCode = courseId,
        };

        var result = await _courseRegistrationServiceClient.GetStudentsForCourseAsync(request);
        
        if (!result.Success)
        {
            throw new CacheServiceException(result.Message);
        }

        // TODO: Make this value custom
        return result.StudentIds.Count >= 18;
    }

    public async Task<bool> EnrollToCourse(string courseId, string studentId)
    {
        var request = new RegisterRequest
        {
            CourseCode = courseId,
            StudentId = studentId,
        };

        var result = await _courseRegistrationServiceClient.RegisterStudentForCourseAsync(request);
        
        if (!result.Success)
        {
            throw new CacheServiceException(result.Message);
        }

        return result.Success;
    }
}