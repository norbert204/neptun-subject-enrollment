using Grpc.Core;
using GrpcDatabaseService.Models;
using GrpcDatabaseService.Protos;
using GrpcDatabaseService.Repositories;
using GrpcDatabaseService.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrpcDatabaseService.Services
{
    /// <summary>
    /// Implementation of the Subject GRPC service
    /// </summary>
    public class SubjectService : Protos.SubjectService.SubjectServiceBase
    {
        private readonly ISubjectRepository _repository;
        private readonly ILogger<SubjectService> _logger;

        /// <summary>
        /// Initializes a new instance of the GrpcSubjectService class
        /// </summary>
        public SubjectService(ISubjectRepository repository, ILogger<SubjectService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new subject
        /// </summary>
        public override async Task<SubjectResponse> CreateSubject(SubjectRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Creating subject with ID: {SubjectId}", request.Id);

            try
            {
                var subject = new Subject
                {
                    Id = request.Id,
                    Owner = request.Owner,
                    Name = request.Name,
                    Prerequisites = request.Prerequisites.ToList(),
                    Courses = request.Courses.ToList()
                };

                var result = await _repository.CreateSubjectAsync(subject);

                return new SubjectResponse
                {
                    Success = true,
                    Message = "Subject created successfully",
                    Subject = new SubjectData
                    {
                        Id = result.Id,
                        Owner = result.Owner,
                        Name = result.Name,
                        Prerequisites = { result.Prerequisites },
                        Courses = { result.Courses }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subject with ID: {SubjectId}", request.Id);
                return new SubjectResponse
                {
                    Success = false,
                    Message = $"Failed to create subject: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Retrieves a subject by ID
        /// </summary>
        public override async Task<SubjectResponse> GetSubject(SubjectIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting subject with ID: {SubjectId}", request.Id);

            try
            {
                var subject = await _repository.GetSubjectAsync(request.Id);
                if (subject == null)
                {
                    return new SubjectResponse
                    {
                        Success = false,
                        Message = "Subject not found"
                    };
                }

                return new SubjectResponse
                {
                    Success = true,
                    Message = "Subject retrieved successfully",
                    Subject = new SubjectData
                    {
                        Id = subject.Id,
                        Owner = subject.Owner,
                        Name = subject.Name,
                        Prerequisites = { subject.Prerequisites },
                        Courses = { subject.Courses }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subject with ID: {SubjectId}", request.Id);
                return new SubjectResponse
                {
                    Success = false,
                    Message = $"Failed to retrieve subject: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Updates an existing subject
        /// </summary>
        public override async Task<SubjectResponse> UpdateSubject(SubjectRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Updating subject with ID: {SubjectId}", request.Id);

            try
            {
                var subject = new Subject
                {
                    Id = request.Id,
                    Owner = request.Owner,
                    Name = request.Name,
                    Prerequisites = request.Prerequisites.ToList(),
                    Courses = request.Courses.ToList()
                };

                var result = await _repository.UpdateSubjectAsync(subject);

                return new SubjectResponse
                {
                    Success = true,
                    Message = "Subject updated successfully",
                    Subject = new SubjectData
                    {
                        Id = result.Id,
                        Owner = result.Owner,
                        Name = result.Name,
                        Prerequisites = { result.Prerequisites ?? [] },
                        Courses = { result.Courses ?? [] }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject with ID: {SubjectId}", request.Id);
                return new SubjectResponse
                {
                    Success = false,
                    Message = $"Failed to update subject: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Deletes a subject by ID
        /// </summary>
        public override async Task<DeleteSubjectResponse> DeleteSubject(SubjectIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Deleting subject with ID: {SubjectId}", request.Id);

            try
            {
                var success = await _repository.DeleteSubjectAsync(request.Id);
                return new DeleteSubjectResponse
                {
                    Success = success,
                    Message = success ? "Subject deleted successfully" : "Failed to delete subject"
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

        /// <summary>
        /// Lists all subjects
        /// </summary>
        public override async Task<SubjectListResponse> ListSubjects(GetAllSubjectsRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Listing all subjects");

            try
            {
                var subjects = await _repository.ListSubjectsAsync();
                var response = new SubjectListResponse();

                foreach (var subject in subjects)
                {
                    response.Subjects.Add(new SubjectData
                    {
                        Id = subject.Id,
                        Owner = subject.Owner,
                        Name = subject.Name,
                        Prerequisites = { subject.Prerequisites ?? [] },
                        Courses = { subject.Courses ?? [] }
                    });
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing subjects");
                return new SubjectListResponse();
            }
        }
    }
}