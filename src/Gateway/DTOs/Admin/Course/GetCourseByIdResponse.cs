namespace Gateway.DTOs.Admin.Course;

public class GetCourseByIdResponse
{
    public string Id { get; init; }
    
    public string Room { get; init; }
    
    public string StartTime { get; init; }
    
    public string EndTime { get; init; }
    
    public int Capacity { get; init; }
    
    public List<string> EnrolledStudents { get; init; }
    
    public string CourseType { get; init; }
}