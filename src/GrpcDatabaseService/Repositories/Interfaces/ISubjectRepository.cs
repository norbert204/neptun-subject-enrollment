using GrpcDatabaseService.Models;

namespace GrpcDatabaseService.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Subject CRUD operations
    /// </summary>
    public interface ISubjectRepository
    {
        /// <summary>
        /// Creates a new subject in the database
        /// </summary>
        Task<Subject> CreateSubjectAsync(Subject subject);

        /// <summary>
        /// Retrieves a subject by its ID
        /// </summary>
        Task<Subject?> GetSubjectAsync(string id);

        /// <summary>
        /// Updates an existing subject
        /// </summary>
        Task<Subject> UpdateSubjectAsync(Subject subject);

        /// <summary>
        /// Deletes a subject by its ID
        /// </summary>
        Task<bool> DeleteSubjectAsync(string id);

        /// <summary>
        /// Retrieves all subjects
        /// </summary>
        Task<List<Subject>> ListSubjectsAsync();
    }
}
