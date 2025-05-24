using Cassandra;
using GrpcDatabaseService.Models;
using GrpcDatabaseService.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using ISession = Cassandra.ISession;

namespace GrpcDatabaseService.Repositories
{
    /// <summary>
    /// Cassandra implementation of the User repository
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ISession _session;
        private readonly ILogger<UserRepository> _logger;
        private readonly PreparedStatement _createStatement;
        private readonly PreparedStatement _getStatement;
        private readonly PreparedStatement _updateStatement;
        private readonly PreparedStatement _deleteStatement;
        private readonly PreparedStatement _listStatement;

        /// <summary>
        /// Initializes a new instance of the UserRepository class
        /// </summary>
        public UserRepository(CassandraConnection connection, ILogger<UserRepository> logger)
        {
            _session = connection.Session;
            _logger = logger;

            // Prepare statements for better performance
            _createStatement = _session.Prepare(
                "INSERT INTO users (neptun_code, name, email, password) VALUES (?, ?, ?, ?)");
            _getStatement = _session.Prepare(
                "SELECT * FROM users WHERE neptun_code = ?");
            _updateStatement = _session.Prepare(
                "UPDATE users SET name = ?, email = ?, password = ? WHERE neptun_code = ?");
            _deleteStatement = _session.Prepare(
                "DELETE FROM users WHERE neptun_code = ?");
            _listStatement = _session.Prepare(
                "SELECT * FROM users");
        }

        /// <inheritdoc/>
        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                // Handle potential apostrophes in the name
                var boundStatement = _createStatement.Bind(
                    user.NeptunCode,
                    user.Name,
                    user.Email,
                    user.Password);

                await _session.ExecuteAsync(boundStatement);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with NEPTUN code: {NeptunCode}", user.NeptunCode);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserAsync(string neptunCode)
        {
            try
            {
                var boundStatement = _getStatement.Bind(neptunCode);
                var rows = await _session.ExecuteAsync(boundStatement);
                var row = rows.FirstOrDefault();

                if (row == null)
                    return null;

                return new User
                {
                    NeptunCode = row.GetValue<string>("neptun_code"),
                    Name = row.GetValue<string>("name"),
                    Email = row.GetValue<string>("email"),
                    Password = row.GetValue<string>("password")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with NEPTUN code: {NeptunCode}", neptunCode);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<User> UpdateUserAsync(User user)
        {
            try
            {
                var boundStatement = _updateStatement.Bind(
                    user.Name,
                    user.Email,
                    user.Password,
                    user.NeptunCode);

                await _session.ExecuteAsync(boundStatement);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with NEPTUN code: {NeptunCode}", user.NeptunCode);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteUserAsync(string neptunCode)
        {
            try
            {
                var boundStatement = _deleteStatement.Bind(neptunCode);
                var result = await _session.ExecuteAsync(boundStatement);
                return result.IsFullyFetched;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with NEPTUN code: {NeptunCode}", neptunCode);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<User>> ListUsersAsync()
        {
            try
            {
                var boundStatement = _listStatement.Bind();
                var rows = await _session.ExecuteAsync(boundStatement);

                var users = new List<User>();
                foreach (var row in rows)
                {
                    users.Add(new User
                    {
                        NeptunCode = row.GetValue<string>("neptun_code"),
                        Name = row.GetValue<string>("name"),
                        Email = row.GetValue<string>("email"),
                        Password = row.GetValue<string>("password")
                    });
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing users");
                throw;
            }
        }
    }
}