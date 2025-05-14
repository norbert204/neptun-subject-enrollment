using Grpc.Core;
using GrpcDatabaseService.Models;
using GrpcDatabaseService.Services;

namespace GrpcDatabaseService.Services
{
    public class UserService : GrpcDatabaseService.UserService.UserServiceBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger<UserService> _logger;

        public UserService(DatabaseContext dbContext, ILogger<UserService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Creating user with NEPTUN code: {NeptunCode}", request.NeptunCode);

            try
            {
                // Create user model from request
                var user = new User
                {
                    NeptunCode = request.NeptunCode,
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password // In a real app, password should be hashed
                };

                await _dbContext.AddUserAsync(user);

                // Create response
                return new CreateUserResponse
                {
                    Success = true,
                    Message = "User created successfully",
                    User = MapToUserDto(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with NEPTUN code: {NeptunCode}", request.NeptunCode);
                return new CreateUserResponse
                {
                    Success = false,
                    Message = $"Failed to create user: {ex.Message}"
                };
            }
        }

        public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting user with NEPTUN code: {NeptunCode}", request.NeptunCode);

            try
            {
                var user = await _dbContext.GetUserByIdAsync(request.NeptunCode);

                if (user == null)
                {
                    return new GetUserResponse
                    {
                        Success = false,
                        Message = $"User with NEPTUN code {request.NeptunCode} not found"
                    };
                }

                return new GetUserResponse
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    User = MapToUserDto(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with NEPTUN code: {NeptunCode}", request.NeptunCode);
                return new GetUserResponse
                {
                    Success = false,
                    Message = $"Failed to retrieve user: {ex.Message}"
                };
            }
        }

        public override async Task<GetAllUsersResponse> GetAllUsers(GetAllUsersRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting all users");

            try
            {
                var users = await _dbContext.GetAllUsersAsync();
                var response = new GetAllUsersResponse
                {
                    Success = true,
                    Message = "Users retrieved successfully"
                };

                foreach (var user in users)
                {
                    response.Users.Add(MapToUserDto(user));
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new GetAllUsersResponse
                {
                    Success = false,
                    Message = $"Failed to retrieve users: {ex.Message}"
                };
            }
        }

        public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Updating user with NEPTUN code: {NeptunCode}", request.NeptunCode);

            try
            {
                var existingUser = await _dbContext.GetUserByIdAsync(request.NeptunCode);

                if (existingUser == null)
                {
                    return new UpdateUserResponse
                    {
                        Success = false,
                        Message = $"User with NEPTUN code {request.NeptunCode} not found"
                    };
                }

                // Update user with new values
                var updatedUser = new User
                {
                    NeptunCode = request.NeptunCode,
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password // In a real app, password should be hashed
                };

                await _dbContext.UpdateUserAsync(updatedUser);

                return new UpdateUserResponse
                {
                    Success = true,
                    Message = "User updated successfully",
                    User = MapToUserDto(updatedUser)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with NEPTUN code: {NeptunCode}", request.NeptunCode);
                return new UpdateUserResponse
                {
                    Success = false,
                    Message = $"Failed to update user: {ex.Message}"
                };
            }
        }

        public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Deleting user with NEPTUN code: {NeptunCode}", request.NeptunCode);

            try
            {
                await _dbContext.DeleteUserAsync(request.NeptunCode);

                return new DeleteUserResponse
                {
                    Success = true,
                    Message = $"User with NEPTUN code {request.NeptunCode} deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with NEPTUN code: {NeptunCode}", request.NeptunCode);
                return new DeleteUserResponse
                {
                    Success = false,
                    Message = $"Failed to delete user: {ex.Message}"
                };
            }
        }

        // Helper method to map from domain model to DTO
        private UserDTO MapToUserDto(User user)
        {
            return new UserDTO
            {
                NeptunCode = user.NeptunCode,
                Name = user.Name,
                Email = user.Email
                // Note: We don't include password in the DTO for security reasons
            };
        }
    }
}