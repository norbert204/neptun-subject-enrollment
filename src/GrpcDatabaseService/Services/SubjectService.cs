using Grpc.Core;
using GrpcDatabaseService.Models;
using GrpcDatabaseService.Services;

namespace GrpcDatabaseService.Services
{
    public class SubjectService : GrpcDatabaseService.SubjectService.SubjectServiceBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger<SubjectService> _logger;

        public SubjectService(DatabaseContext dbContext, ILogger<SubjectService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<CreateSubjectResponse> CreateSubject(CreateSubjectRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Creating subject with ID: {SubjectId}", request.Id);

            try
            {
                // Create subject model from request
                var subject = new Subject
                {
                    ID = request.Id,
                    Owner = request.Owner,
                    Name = request.Name,
                    Prerequisites = new System.Collections.Generic.List<string>(request.Prerequisites),
                    Courses = new System.Collections.Generic.List<string>(request.Courses)
                };

                await _dbContext.AddSubjectAsync(subject);

                // Create response
                return new CreateSubjectResponse
                {
                    Success = true,
                    Message = "Subject created successfully",
                    Subject = MapToSubjectDto(subject)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subject with ID: {SubjectId}", request.Id);
                return new CreateSubjectResponse
                {
                    Success = false,
                    Message = $"Failed to create subject: {ex.Message}"
                };
            }
        }

        public override async Task<GetSubjectResponse> GetSubject(GetSubjectRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting subject with ID: {SubjectId}", request.Id);

            try
            {
                var subject = await _dbContext.GetSubjectByIdAsync(request.Id);

                if (subject == null)
                {
                    return new GetSubjectResponse
                    {
                        Success = false,
                        Message = $"Subject with ID {request.Id} not found"
                    };
                }

                return new GetSubjectResponse
                {
                    Success = true,
                    Message = "Subject retrieved successfully",
                    Subject = MapToSubjectDto(subject)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subject with ID: {SubjectId}", request.Id);
                return new GetSubjectResponse
                {
                    Success = false,
                    Message = $"Failed to retrieve subject: {ex.Message}"
                };
            }
        }

        public override async Task<GetAllSubjectsResponse> GetAllSubjects(GetAllSubjectsRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting all subjects");

            try
            {
                var subjects = await _dbContext.GetAllSubjectsAsync();
                var response = new GetAllSubjectsResponse
                {
                    Success = true,
                    Message = "Subjects retrieved successfully"
                };

                foreach (var subject in subjects)
                {
                    response.Subjects.Add(MapToSubjectDto(subject));
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all subjects");
                return new GetAllSubjectsResponse
                {
                    Success = false,
                    Message = $"Failed to retrieve subjects: {ex.Message}"
                };
            }
        }

        public override async Task<UpdateSubjectResponse> UpdateSubject(UpdateSubjectRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Updating subject with ID: {SubjectId}", request.Id);

            try
            {
                var existingSubject = await _dbContext.GetSubjectByIdAsync(request.Id);

                if (existingSubject == null)
                {
                    return new UpdateSubjectResponse
                    {
                        Success = false,
                        Message = $"Subject with ID {request.Id} not found"
                    };
                }

                // Update subject with new values
                var updatedSubject = new Subject
                {
                    ID = request.Id,
                    Owner = request.Owner,
                    Name = request.Name,
                    Prerequisites = new System.Collections.Generic.List<string>(request.Prerequisites),
                    Courses = new System.Collections.Generic.List<string>(request.Courses)
                };

                await _dbContext.UpdateSubjectAsync(updatedSubject);

                return new UpdateSubjectResponse
                {
                    Success = true,
                    Message = "Subject updated successfully",
                    Subject = MapToSubjectDto(updatedSubject)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject with ID: {SubjectId}", request.Id);
                return new UpdateSubjectResponse
                {
                    Success = false,
                    Message = $"Failed to update subject: {ex.Message}"
                };
            }
        }

        public override async Task<DeleteSubjectResponse> DeleteSubject(DeleteSubjectRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Deleting subject with ID: {SubjectId}", request.Id);

            try
            {
                await _dbContext.DeleteSubjectAsync(request.Id);

                return new DeleteSubjectResponse
                {
                    Success = true,
                    Message = $"Subject with ID {request.Id} deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subject with ID: {SubjectId}", request.Id);
                return new DeleteSubjectResponse
                {
                    Success = false,
                    Message = $"Failed to delete subject: {ex.Message}"
                };
            }
        }

        public override async Task<AddCourseResponse> AddCourse(AddCourseRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Adding course {CourseId} to subject {SubjectId}", request.CourseId, request.SubjectId);

            try
            {
                await _dbContext.AddCourseToSubjectAsync(request.SubjectId, request.CourseId);
                var updatedSubject = await _dbContext.GetSubjectByIdAsync(request.SubjectId);

                return new AddCourseResponse
                {
                    Success = true,
                    Message = $"Course {request.CourseId} added to subject {request.SubjectId} successfully",
                    Subject = MapToSubjectDto(updatedSubject)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding course {CourseId} to subject {SubjectId}", request.CourseId, request.SubjectId);
                return new AddCourseResponse
                {
                    Success = false,
                    Message = $"Failed to add course to subject: {ex.Message}"
                };
            }
        }

        public override async Task<RemoveCourseResponse> RemoveCourse(RemoveCourseRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Removing course {CourseId} from subject {SubjectId}", request.CourseId, request.SubjectId);

            try
            {
                await _dbContext.RemoveCourseFromSubjectAsync(request.SubjectId, request.CourseId);
                var updatedSubject = await _dbContext.GetSubjectByIdAsync(request.SubjectId);

                return new RemoveCourseResponse
                {
                    Success = true,
                    Message = $"Course {request.CourseId} removed from subject {request.SubjectId} successfully",
                    Subject = MapToSubjectDto(updatedSubject)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing course {CourseId} from subject {SubjectId}", request.CourseId, request.SubjectId);
                return new RemoveCourseResponse
                {
                    Success = false,
                    Message = $"Failed to remove course from subject: {ex.Message}"
                };
            }
        }

        public override async Task<AddPrerequisiteResponse> AddPrerequisite(AddPrerequisiteRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Adding prerequisite {PrerequisiteId} to subject {SubjectId}", request.PrerequisiteId, request.SubjectId);

            try
            {
                await _dbContext.AddPrerequisiteToSubjectAsync(request.SubjectId, request.PrerequisiteId);
                var updatedSubject = await _dbContext.GetSubjectByIdAsync(request.SubjectId);

                return new AddPrerequisiteResponse
                {
                    Success = true,
                    Message = $"Prerequisite {request.PrerequisiteId} added to subject {request.SubjectId} successfully",
                    Subject = MapToSubjectDto(updatedSubject)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding prerequisite {PrerequisiteId} to subject {SubjectId}", request.PrerequisiteId, request.SubjectId);
                return new AddPrerequisiteResponse
                {
                    Success = false,
                    Message = $"Failed to add prerequisite to subject: {ex.Message}"
                };
            }
        }

        public override async Task<RemovePrerequisiteResponse> RemovePrerequisite(RemovePrerequisiteRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Removing prerequisite {PrerequisiteId} from subject {SubjectId}", request.PrerequisiteId, request.SubjectId);

            try
            {
                await _dbContext.RemovePrerequisiteFromSubjectAsync(request.SubjectId, request.PrerequisiteId);
                var updatedSubject = await _dbContext.GetSubjectByIdAsync(request.SubjectId);

                return new RemovePrerequisiteResponse
                {
                    Success = true,
                    Message = $"Prerequisite {request.PrerequisiteId} removed from subject {request.SubjectId} successfully",
                    Subject = MapToSubjectDto(updatedSubject)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing prerequisite {PrerequisiteId} from subject {SubjectId}", request.PrerequisiteId, request.SubjectId);
                return new RemovePrerequisiteResponse
                {
                    Success = false,
                    Message = $"Failed to remove prerequisite from subject: {ex.Message}"
                };
            }
        }

        // Helper method to map from domain model to DTO
        private SubjectDTO MapToSubjectDto(Subject subject)
        {
            var dto = new SubjectDTO
            {
                Id = subject.ID,
                Owner = subject.Owner,
                Name = subject.Name
            };

            // Add prerequisites
            foreach (var prerequisite in subject.Prerequisites)
            {
                dto.Prerequisites.Add(prerequisite);
            }

            // Add courses
            foreach (var course in subject.Courses)
            {
                dto.Courses.Add(course);
            }

            return dto;
        }
    }
}