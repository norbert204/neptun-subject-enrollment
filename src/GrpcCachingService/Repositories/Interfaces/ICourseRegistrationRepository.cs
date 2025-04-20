namespace GrpcCachingService.Repositories.Interfaces;

public interface ICourseRegistrationRepository
{
    Task<bool> InitializeCourseAsync(string courseCode, int maxEnrollment = 10);
    Task<(bool Success, string Message)> RegisterStudentForCourseAsync(string courseCode, string studentId);
    Task<bool> UnregisterStudentFromCourseAsync(string courseCode, string studentId);
    Task<IEnumerable<string>> GetStudentsForCourseAsync(string courseCode);
    Task<IEnumerable<string>> GetCoursesForStudentAsync(string studentId);
    Task<(int CurrentEnrollment, int MaxEnrollment, bool IsFull)> GetCourseEnrollmentStatusAsync(string courseCode);
    Task<bool> InitializeStudentEligibleCoursesAsync(string studentId, IEnumerable<string> eligibleCourseCodes);
    Task<IEnumerable<string>> GetEligibleCoursesForStudentAsync(string studentId);
}

