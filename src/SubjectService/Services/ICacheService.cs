namespace NeptunKiller.SubjectService.Services;

public interface ICacheService
{
    Task<bool> CanStudentEnrollToCourseAsync(string courseId, string studentId);

    Task<bool> IsCourseFullAsync(string courseId);
    
    Task<bool> EnrollToCourseAsync(string courseId, string studentId);
    
    Task<bool> IsStudentAlreadyEnrolledAsync(string courseId, string studentId);
}