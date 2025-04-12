namespace NeptunKiller.SubjectService.Services;

public class DummyCacheService : ICacheService
{
    public Task<bool> CanStudentEnrollToCourse(string courseId, string studentId)
    {
        return Task.FromResult(courseId == "2" && studentId == "1");
    }

    public Task<bool> IsCourseFull(string courseId)
    {
        return Task.FromResult(courseId == "1");
    }

    public Task<bool> EnrollToCourse(string courseId, string studentId)
    {
        return Task.FromResult(courseId == "2" && studentId == "1");
    }
}