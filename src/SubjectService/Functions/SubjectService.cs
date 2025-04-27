using Grpc.Core;
using NeptunKiller.SubjectService.Exceptions;
using NeptunKiller.SubjectService.Services;
using SubjectService;

namespace NeptunKiller.SubjectService.Functions;

public class SubjectService : Subject.SubjectBase
{
    private readonly ILogger<SubjectService> _logger;
    private readonly ICacheService _cacheService;

    public SubjectService(ILogger<SubjectService> logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public override async Task<EnrollToCourseResponse> EnrollToCourse(EnrollToCourseRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Enrolling student {StudentId} to course {CourseId}", request.StudentId, request.CourseId);

        try
        {
            if (await _cacheService.IsCourseFull(request.CourseId))
            {
                return new EnrollToCourseResponse
                {
                    Success = false,
                    Message = "Course is full",
                };
            }

            if (await _cacheService.IsStudentAlreadyEnrolled(request.StudentId, request.CourseId))
            {
                return new EnrollToCourseResponse
                {
                    Success = false,
                    Message = "Student is already enrolled to this course",
                };
            }

            if (!await _cacheService.CanStudentEnrollToCourse(request.StudentId, request.CourseId))
            {
                return new EnrollToCourseResponse
                {
                    Success = false,
                    Message = "Student is not allowed to enroll to this course",
                };
            }

            if (!await _cacheService.EnrollToCourse(request.CourseId, request.StudentId))
            {
                return new EnrollToCourseResponse
                {
                    Success = false,
                    Message = "Course is full",
                };
            }

            return new EnrollToCourseResponse
            {
                Success = true,
                Message = "Successful enrollment",
            };
        }
        catch (CacheServiceException ex)
        {
            return new EnrollToCourseResponse
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }
}