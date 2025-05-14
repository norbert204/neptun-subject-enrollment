using Grpc.Core;
using GrpcDatabaseService.Models;
using GrpcDatabaseService.Services;

namespace GrpcDatabaseService.Services
{
    public class CourseService : GrpcDatabaseService.CourseService.CourseServiceBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger<CourseService> _logger;

        public CourseService(DatabaseContext dbContext, ILogger<CourseService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public override async Task<CreateCourseResponse> CreateCourse(CreateCourseRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Creating course with ID: {CourseId}", request.Id);

            try
            {
                // Create course model from request
                var course = new Course
                {
                    ID = request.Id,
                    Room = request.Room,
                    StartTime = $"{request.StartTimeHours}:{request.StartTimeMinutes}",
                    EndTime = $"{request.EndTimeHours:D2}:{request.EndTimeMinutes:D2}",
                    Capacity = request.Capacity,
                    EnrolledStudents = new System.Collections.Generic.List<string>(),
                    CourseType = (Models.CourseType)request.CourseType
                };

                await _dbContext.AddCourseAsync(course);

                // Create response
                return new CreateCourseResponse
                {
                    Success = true,
                    Message = "Course created successfully",
                    Course = MapToCourseDto(course)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course with ID: {CourseId}", request.Id);
                return new CreateCourseResponse
                {
                    Success = false,
                    Message = $"Failed to create course: {ex.Message}"
                };
            }
        }

        public override async Task<GetCourseResponse> GetCourse(GetCourseRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting course with ID: {CourseId}", request.Id);

            try
            {
                var course = await _dbContext.GetCourseByIdAsync(request.Id);

                if (course == null)
                {
                    return new GetCourseResponse
                    {
                        Success = false,
                        Message = $"Course with ID {request.Id} not found"
                    };
                }

                return new GetCourseResponse
                {
                    Success = true,
                    Message = "Course retrieved successfully",
                    Course = MapToCourseDto(course)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course with ID: {CourseId}", request.Id);
                return new GetCourseResponse
                {
                    Success = false,
                    Message = $"Failed to retrieve course: {ex.Message}"
                };
            }
        }
        public override async Task<GetAllCoursesResponse> GetAllCourses(GetAllCoursesRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting all courses");

            try
            {
                var courses = await _dbContext.GetAllCoursesAsync();
                var response = new GetAllCoursesResponse
                {
                    Success = true,
                    Message = "Courses retrieved successfully"
                };

                foreach (var course in courses)
                {
                    response.Courses.Add(MapToCourseDto(course));
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all courses");
                return new GetAllCoursesResponse
                {
                    Success = false,
                    Message = $"Failed to retrieve courses: {ex.Message}"
                };
            }
        }
        public override async Task<UpdateCourseResponse> UpdateCourse(UpdateCourseRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Updating course with ID: {CourseId}", request.Id);

            try
            {
                var existingCourse = await _dbContext.GetCourseByIdAsync(request.Id);

                if (existingCourse == null)
                {
                    return new UpdateCourseResponse
                    {
                        Success = false,
                        Message = $"Course with ID {request.Id} not found"
                    };
                }

                // Update course with new values but preserve enrolled students
                var updatedCourse = new Course
                {
                    ID = request.Id,
                    Room = request.Room,
                    StartTime = $"{request.StartTimeHours:D2}:{request.StartTimeMinutes:D2}",
                    EndTime = $"{request.EndTimeHours:D2}:{request.EndTimeMinutes:D2}",
                    Capacity = request.Capacity,
                    EnrolledStudents = existingCourse.EnrolledStudents, // Preserve existing enrolled students
                    CourseType = (Models.CourseType)request.CourseType
                };

                await _dbContext.UpdateCourseAsync(updatedCourse);

                return new UpdateCourseResponse
                {
                    Success = true,
                    Message = "Course updated successfully",
                    Course = MapToCourseDto(updatedCourse)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course with ID: {CourseId}", request.Id);
                return new UpdateCourseResponse
                {
                    Success = false,
                    Message = $"Failed to update course: {ex.Message}"
                };
            }
        }
        public override async Task<DeleteCourseResponse> DeleteCourse(DeleteCourseRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Deleting course with ID: {CourseId}", request.Id);

            try
            {
                await _dbContext.DeleteCourseAsync(request.Id);

                return new DeleteCourseResponse
                {
                    Success = true,
                    Message = $"Course with ID {request.Id} deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course with ID: {CourseId}", request.Id);
                return new DeleteCourseResponse
                {
                    Success = false,
                    Message = $"Failed to delete course: {ex.Message}"
                };
            }
        }
        public override async Task<EnrollStudentResponse> EnrollStudent(EnrollStudentRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Enrolling student {NeptunCode} in course {CourseId}", request.NeptunCode, request.CourseId);
            
            try
            {
                await _dbContext.EnrollStudentAsync(request.CourseId, request.NeptunCode);
                var updatedCourse = await _dbContext.GetCourseByIdAsync(request.CourseId);
                
                return new EnrollStudentResponse
                {
                    Success = true,
                    Message = $"Student {request.NeptunCode} enrolled successfully",
                    Course = MapToCourseDto(updatedCourse)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling student {NeptunCode} in course {CourseId}", request.NeptunCode, request.CourseId);
                return new EnrollStudentResponse
                {
                    Success = false,
                    Message = $"Failed to enroll student: {ex.Message}"
                };
            }
        }
        public override async Task<RemoveStudentResponse> RemoveStudent(RemoveStudentRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Removing student {NeptunCode} from course {CourseId}", request.NeptunCode, request.CourseId);
            
            try
            {
                await _dbContext.RemoveStudentAsync(request.CourseId, request.NeptunCode);
                var updatedCourse = await _dbContext.GetCourseByIdAsync(request.CourseId);
                
                return new RemoveStudentResponse
                {
                    Success = true,
                    Message = $"Student {request.NeptunCode} removed successfully",
                    Course = MapToCourseDto(updatedCourse)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing student {NeptunCode} from course {CourseId}", request.NeptunCode, request.CourseId);
                return new RemoveStudentResponse
                {
                    Success = false,
                    Message = $"Failed to remove student: {ex.Message}"
                };
            }
        }

        private CourseDTO MapToCourseDto(Course course)
        {
            var dto = new CourseDTO
            {
                Id = course.ID,
                Room = course.Room,
                StartTime = course.StartTime,
                EndTime = course.EndTime,
                Capacity = course.Capacity,
                CourseType = (CourseType)course.CourseType
            };

            // Add enrolled students
            foreach (var student in course.EnrolledStudents)
            {
                dto.EnrolledStudents.Add(student);
            }

            return dto;
        }
    }
}