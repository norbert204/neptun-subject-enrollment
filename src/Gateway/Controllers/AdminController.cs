using System.Net;
using Gateway.DTOs.Admin.Course;
using Gateway.DTOs.Subject;
using Google.Protobuf.WellKnownTypes;
using GrpcDatabaseService.Protos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SubjectService;

namespace Gateway.Controllers;

[Route("[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly Subject.SubjectClient _subjectServiceClient;
    private readonly GrpcDatabaseService.Protos.SubjectService.SubjectServiceClient _databaseSubjectServiceClient;
    private readonly CourseService.CourseServiceClient _databaseCourseServiceClient;
    private readonly UserService.UserServiceClient _databaseUserServiceClient;

    public AdminController(
        Subject.SubjectClient subjectServiceClient,
        ILogger<AdminController> logger,
        GrpcDatabaseService.Protos.SubjectService.SubjectServiceClient databaseSubjectServiceClient,
        CourseService.CourseServiceClient databaseCourseServiceClient,
        UserService.UserServiceClient databaseUserServiceClient)
    {
        _subjectServiceClient = subjectServiceClient;
        _logger = logger;
        _databaseSubjectServiceClient = databaseSubjectServiceClient;
        _databaseCourseServiceClient = databaseCourseServiceClient;
        _databaseUserServiceClient = databaseUserServiceClient;
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

    [HttpPost("course")]
    public async Task<IActionResult> AddCourseAsync(CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var serviceRequest = new CourseRequest
        {
            Id = request.Id,
            Room = request.Room,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity,
            EnrolledStudents = { Enumerable.Empty<string>() },
            CourseType = request.CourseType,
        };

        var response = await _databaseCourseServiceClient.CreateCourseAsync(serviceRequest, cancellationToken: cancellationToken);

        if (!response.Success)
        {
            return StatusCode(
                500,
                new ProblemDetails
                {
                    Detail = response.Message,
                    Status = (int)HttpStatusCode.InternalServerError,
                });
        }

        return Ok();
    }

    [HttpGet("course/{courseId}")]
    public async Task<ActionResult<GetCourseByIdResponse>> GetCourseAsync(string courseId, CancellationToken cancellationToken)
    {
        var request = new CourseIdRequest
        {
            Id = courseId,
        };

        var response = await _databaseCourseServiceClient.GetCourseAsync(request, cancellationToken: cancellationToken);
        
        if (!response.Success)
        {
            return StatusCode(
                500,
                new ProblemDetails
                {
                    Detail = response.Message,
                    Status = (int)HttpStatusCode.InternalServerError,
                });
        }

        var result = new GetCourseByIdResponse
        {
            Id = response.Course.Id,
            Room = response.Course.Room,
            StartTime = response.Course.StartTime,
            EndTime = response.Course.EndTime,
            Capacity = response.Course.Capacity,
            CourseType = response.Course.CourseType,
            EnrolledStudents = response.Course.EnrolledStudents.ToList(),
        };

        return Ok(result);
    }

    [HttpPut("course")]
    public async Task<IActionResult> UpdateCourseAsync(UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var serviceRequest = new CourseRequest
        {
            Id = request.Id,
            Room = request.Room,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity,
            CourseType = request.CourseType,
            EnrolledStudents = { request.EnrolledStudents },
        };

        var response = await _databaseCourseServiceClient.UpdateCourseAsync(serviceRequest, cancellationToken: cancellationToken);
        
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

    [HttpDelete("course/{courseId}")]
    public async Task<IActionResult> DeleteCourseAsync(string courseId, CancellationToken cancellationToken)
    {
        var request = new CourseIdRequest
        {
            Id = courseId,
        };

        var response = await _databaseCourseServiceClient.DeleteCourseAsync(request, cancellationToken: cancellationToken);
        
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

    [HttpGet("course")]
    public async Task<ActionResult<ListCoursesResponse>> ListCoursesAsync(CancellationToken cancellationToken)
    {
        var request = new GetAllCoursesRequest();

        var response = await _databaseCourseServiceClient.ListCoursesAsync(request, cancellationToken: cancellationToken);

        var result = response.Courses
            .Select(x => new AdminCourseDto
            {
                Id = x.Id,
                Room = x.Room,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Capacity = x.Capacity,
                CourseType = x.CourseType,
                EnrolledStudents = x.EnrolledStudents.ToList(),
            })
            .ToList();
        return Ok(new ListCoursesResponse{  Courses = result });
    }
}