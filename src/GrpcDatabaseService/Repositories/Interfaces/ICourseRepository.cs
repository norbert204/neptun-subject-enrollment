using GrpcDatabaseService.Models;

namespace GrpcDatabaseService.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Course CRUD operations
    /// </summary>
    public interface ICourseRepository
    {
        /// <summary>
        /// Creates a new course in the database
        /// </summary>
        Task<Course> CreateCourseAsync(Course course);

        /// <summary>
        /// Retrieves a course by its ID
        /// </summary>
        Task<Course?> GetCourseAsync(string id);

        /// <summary>
        /// Updates an existing course
        /// </summary>
        Task<Course> UpdateCourseAsync(Course course);

        /// <summary>
        /// Deletes a course by its ID
        /// </summary>
        Task<bool> DeleteCourseAsync(string id);

        /// <summary>
        /// Retrieves all courses
        /// </summary>
        Task<List<Course>> ListCoursesAsync();
    }
}
