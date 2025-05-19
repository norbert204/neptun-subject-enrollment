using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcDatabaseService.Models;
using GrpcDatabaseService.Protos;
using GrpcDatabaseService.Repositories;
using GrpcDatabaseService.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrpcDatabaseService.Services
{
    /// <summary>
    /// Implementation of the Course GRPC service
    /// </summary>
    public class CourseService : Protos.CourseService.CourseServiceBase
    {
        private readonly ICourseRepository _repository;
        private readonly ILogger<CourseService> _logger;

        /// <summary>
        /// Initializes a new instance of the CourseService class
        /// </summary>
        public CourseService(ICourseRepository repository, ILogger<CourseService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new course
        /// </summary>
        public override async Task<CourseResponse> CreateCourse(CourseRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Creating course with ID: {CourseId}", request.Id);

            try
            {
                var course = new Course
                {
                    Id = request.Id,
                    Room = request.Room,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Capacity = request.Capacity,
                    EnrolledStudents = request.EnrolledStudents.ToList(),
                    CourseType = request.CourseType
                };

                var result = await _repository.CreateCourseAsync(course);

                return new CourseResponse
                {
                    Success = true,
                    Message = "Course created successfully",
                    Course = new CourseData
                    {
                        Id = result.Id,
                        Room = result.Room,
                        StartTime = result.StartTime,
                        EndTime = result.EndTime,
                        Capacity = result.Capacity,
                        EnrolledStudents = { result.EnrolledStudents },
                        CourseType = result.CourseType
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course with ID: {CourseId}", request.Id);
                return new CourseResponse
                {
                    Success = false,
                    Message = $"Failed to create course: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Retrieves a course by ID
        /// </summary>
        public override async Task<CourseResponse> GetCourse(CourseIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting course with ID: {CourseId}", request.Id);

            try
            {
                var course = await _repository.GetCourseAsync(request.Id);
                if (course == null)
                {
                    return new CourseResponse
                    {
                        Success = false,
                        Message = "Course not found"
                    };
                }

                return new CourseResponse
                {
                    Success = true,
                    Message = "Course retrieved successfully",
                    Course = new CourseData
                    {
                        Id = course.Id,
                        Room = course.Room,
                        StartTime = course.StartTime,
                        EndTime = course.EndTime,
                        Capacity = course.Capacity,
                        EnrolledStudents = { course.EnrolledStudents },
                        CourseType = course.CourseType
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course with ID: {CourseId}", request.Id);
                return new CourseResponse
                {
                    Success = false,
                    Message = $"Failed to retrieve course: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Updates an existing course
        /// </summary>
        public override async Task<CourseResponse> UpdateCourse(CourseRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Updating course with ID: {CourseId}", request.Id);

            try
            {
                var course = new Course
                {
                    Id = request.Id,
                    Room = request.Room,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Capacity = request.Capacity,
                    EnrolledStudents = request.EnrolledStudents.ToList(),
                    CourseType = request.CourseType
                };

                var result = await _repository.UpdateCourseAsync(course);

                return new CourseResponse
                {
                    Success = true,
                    Message = "Course updated successfully",
                    Course = new CourseData
                    {
                        Id = result.Id,
                        Room = result.Room,
                        StartTime = result.StartTime,
                        EndTime = result.EndTime,
                        Capacity = result.Capacity,
                        EnrolledStudents = { result.EnrolledStudents },
                        CourseType = result.CourseType
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course with ID: {CourseId}", request.Id);
                return new CourseResponse
                {
                    Success = false,
                    Message = $"Failed to update course: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Deletes a course by ID
        /// </summary>
        public override async Task<DeleteCourseResponse> DeleteCourse(CourseIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Deleting course with ID: {CourseId}", request.Id);

            try
            {
                var success = await _repository.DeleteCourseAsync(request.Id);
                return new DeleteCourseResponse
                {
                    Success = success,
                    Message = success ? "Course deleted successfully" : "Failed to delete course"
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

        /// <summary>
        /// Lists all courses
        /// </summary>
        public override async Task<CourseListResponse> ListCourses(GetAllCoursesRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Listing all courses");

            try
            {
                var courses = await _repository.ListCoursesAsync();
                var response = new CourseListResponse();

                foreach (var course in courses)
                {
                    response.Courses.Add(new CourseData
                    {
                        Id = course.Id,
                        Room = course.Room,
                        StartTime = course.StartTime,
                        EndTime = course.EndTime,
                        Capacity = course.Capacity,
                        EnrolledStudents = { course.EnrolledStudents },
                        CourseType = course.CourseType
                    });
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing courses");
                return new CourseListResponse();
            }
        }
    }
}