using CourseRegistrationService.External;

namespace GrpcCachingService.Services.Interfaces;

public interface ICourseDataServiceClient
{
    Task<GetAllCoursesResponse> GetAllCoursesAsync();
    Task<GetStudentsResponse> GetStudentsWithEligibleCoursesAsync();
}