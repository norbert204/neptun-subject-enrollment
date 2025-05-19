using GrpcDatabaseService.Models;

namespace GrpcDatabaseService.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for User CRUD operations
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Creates a new user in the database
        /// </summary>
        Task<User> CreateUserAsync(User user);

        /// <summary>
        /// Retrieves a user by their NEPTUN code
        /// </summary>
        Task<User?> GetUserAsync(string neptunCode);

        /// <summary>
        /// Updates an existing user
        /// </summary>
        Task<User> UpdateUserAsync(User user);

        /// <summary>
        /// Deletes a user by their NEPTUN code
        /// </summary>
        Task<bool> DeleteUserAsync(string neptunCode);

        /// <summary>
        /// Retrieves all users
        /// </summary>
        Task<List<User>> ListUsersAsync();
    }
}
