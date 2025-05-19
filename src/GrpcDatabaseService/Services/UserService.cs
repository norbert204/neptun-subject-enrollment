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
    /// Implementation of the User GRPC service
    /// </summary>
    public class UserService : Protos.UserService.UserServiceBase
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// Initializes a new instance of the UserService class
        /// </summary>
        public UserService(IUserRepository repository, ILogger<UserService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        public override async Task<UserResponse> CreateUser(UserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Creating user with NEPTUN code: {NeptunCode}", request.NeptunCode);

            try
            {
                var user = new User
                {
                    NeptunCode = request.NeptunCode,
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password
                };

                var result = await _repository.CreateUserAsync(user);

                return new UserResponse
                {
                    Success = true,
                    Message = "User created successfully",
                    User = new UserData
                    {
                        NeptunCode = result.NeptunCode,
                        Name = result.Name,
                        Email = result.Email
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with NEPTUN code: {NeptunCode}", request.NeptunCode);
                return new UserResponse
                {
                    Success = false,
                    Message = $"Failed to create user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Retrieves a user by NEPTUN code
        /// </summary>
        public override async Task<UserResponse> GetUser(UserIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting user with NEPTUN code: {NeptunCode}", request.NeptunCode);

            try
            {
                var user = await _repository.GetUserAsync(request.NeptunCode);
                if (user == null)
                {
                    return new UserResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                return new UserResponse
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    User = new UserData
                    {
                        NeptunCode = user.NeptunCode,
                        Name = user.Name,
                        Email = user.Email
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with NEPTUN code: {NeptunCode}", request.NeptunCode);
                return new UserResponse
                {
                    Success = false,
                    Message = $"Failed to retrieve user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        public override async Task<UserResponse> UpdateUser(UserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Updating user with NEPTUN code: {NeptunCode}", request.NeptunCode);

            try
            {
                var user = new User
                {
                    NeptunCode = request.NeptunCode,
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password
                };

                var result = await _repository.UpdateUserAsync(user);

                return new UserResponse
                {
                    Success = true,
                    Message = "User updated successfully",
                    User = new UserData
                    {
                        NeptunCode = result.NeptunCode,
                        Name = result.Name,
                        Email = result.Email
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with NEPTUN code: {NeptunCode}", request.NeptunCode);
                return new UserResponse
                {
                    Success = false,
                    Message = $"Failed to update user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Deletes a user by NEPTUN code
        /// </summary>
        public override async Task<DeleteResponse> DeleteUser(UserIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Deleting user with NEPTUN code: {NeptunCode}", request.NeptunCode);

            try
            {
                var success = await _repository.DeleteUserAsync(request.NeptunCode);
                return new DeleteResponse
                {
                    Success = success,
                    Message = success ? "User deleted successfully" : "Failed to delete user"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with NEPTUN code: {NeptunCode}", request.NeptunCode);
                return new DeleteResponse
                {
                    Success = false,
                    Message = $"Failed to delete user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Lists all users
        /// </summary>
        public override async Task<UserListResponse> ListUsers(GetAllUsersRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Listing all users");

            try
            {
                var users = await _repository.ListUsersAsync();
                var response = new UserListResponse();

                foreach (var user in users)
                {
                    response.Users.Add(new UserData
                    {
                        NeptunCode = user.NeptunCode,
                        Name = user.Name,
                        Email = user.Email
                    });
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing users");
                return new UserListResponse();
            }
        }
    }
}