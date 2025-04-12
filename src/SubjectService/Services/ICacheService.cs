namespace NeptunKiller.SubjectService.Services;

public interface ICacheService
{
    Task<bool> CanStudentEnrollToCourse(string courseId, string studentId);

    Task<bool> IsCourseFull(string courseId);
    
    Task<bool> EnrollToCourse(string courseId, string studentId);
}