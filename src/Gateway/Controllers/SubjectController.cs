using Gateway.DTOs.Subject;
using Microsoft.AspNetCore.Mvc;
using SubjectService;

namespace Gateway.Controllers;

[Route("[controller]")]
[ApiController]
public class SubjectController : ControllerBase
{
    private readonly ILogger<SubjectController> _logger;
    private readonly Subject.SubjectClient _subjectServiceClient;

    public SubjectController(ILogger<SubjectController> logger, Subject.SubjectClient subjectServiceClient)
    {
        _logger = logger;
        _subjectServiceClient = subjectServiceClient;
    }

    [HttpGet("eligible-courses")]
    public async Task<ActionResult<EligibleCoursesResponse>> GetEligibleCoursesForStudentAsync(string studentId, CancellationToken cancellationToken)
    {
        var request = new ListEligibleCoursesRequest
        {
            StudentId = studentId,
        };

        var response = await _subjectServiceClient.ListEligibleCoursesAsync(request, cancellationToken: cancellationToken);

        var result = response.Courses
            .Select(x => new CourseDto
            {
                CourseId = x.CourseId,
                CourseType = x.CourseType,
                EndTime = x.EndTime,
                Room = x.CourseRoom,
                StartTime = x.StartTime,
            })
            .ToList();
        
        return Ok(new EligibleCoursesResponse{ EligibleCourses = result });
    }
}