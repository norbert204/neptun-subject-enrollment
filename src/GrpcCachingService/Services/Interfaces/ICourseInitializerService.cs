namespace GrpcCachingService.Services.Interfaces;

public interface ICourseInitializerService
{
    Task<(int InitializedCount, bool Success, string Message)> InitializeCoursesAsync(bool forceUpdate = false);
    Task<(int InitializedCount, bool Success, string Message)> InitializeStudentsAsync(bool forceUpdate = false);
    Task<(int InitializedCourseCount, int InitializedStudentCount, bool Success, string Message)> InitializeAllAsync(bool forceUpdate = false);
}
