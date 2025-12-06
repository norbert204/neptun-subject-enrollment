using System.Net;
using Gateway.DTOs.Subject;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using SubjectService;
using EnrollToCourseRequest = Gateway.DTOs.Subject.EnrollToCourseRequest;

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

    [HttpPost("enroll-to-course")]
    public async Task<IActionResult> EnrollToCourseAsync(EnrollToCourseRequest request, CancellationToken cancellationToken)
    {
        var serviceRequest = new SubjectService.EnrollToCourseRequest
        {
            CourseId = request.CourseId,
            StudentId = request.StudentId,
        };

        var response = await _subjectServiceClient.EnrollToCourseAsync(serviceRequest, cancellationToken: cancellationToken);

        if (!response.Success)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Detail = response.Message,
                    Status = (int)HttpStatusCode.BadRequest,
                });
        }

        return Ok();
    }

    [HttpPost("start-enrollment-period")]
    public async Task<IActionResult> StartEnrollmentPeriodAsync(CancellationToken cancellationToken)
    {
        var response = await _subjectServiceClient.InitializeSubjectEnrollmentAsync(new Empty(), cancellationToken: cancellationToken);

        if (!response.Success)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Detail = response.Message,
                    Status = (int)HttpStatusCode.BadRequest,
                });
        }

        return Ok();
    }
}