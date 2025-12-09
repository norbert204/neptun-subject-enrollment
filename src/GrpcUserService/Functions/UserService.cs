using Grpc.Core;
using GrpcDatabaseService.Protos;
using UserService;
using DeleteResponse = UserService.DeleteResponse;
using UserData = UserService.UserData;
using UserListResponse = UserService.UserListResponse;
using UserResponse = UserService.UserResponse;

namespace NeptunKiller.UserService.Functions
{
    public class UserService : User.UserBase
    {
        private readonly ILogger<UserService> _logger;
        private readonly GrpcDatabaseService.Protos.UserService.UserServiceClient _databaseUserService;

        public UserService(
            ILogger<UserService> logger,
            GrpcDatabaseService.Protos.UserService.UserServiceClient databaseUserService)
        {
            _logger = logger;
            _databaseUserService = databaseUserService;
        }

        public override async Task<UserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Creating user with neptun code: {NeptunCode}", request.NeptunCode);

            try
            {
                var databaseRequest = new UserRequest
                {
                    NeptunCode = request.NeptunCode,
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password
                };

                var databaseResponse = await _databaseUserService.CreateUserAsync(databaseRequest);

                return new UserResponse
                {
                    Success = databaseResponse.Success,
                    Message = databaseResponse.Message,
                    User = databaseResponse.User != null ? new UserData
                    {
                        NeptunCode = databaseResponse.User.NeptunCode,
                        Name = databaseResponse.User.Name,
                        Email = databaseResponse.User.Email,
                    } : null
                };
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error creating user with neptun code: {NeptunCode}", request.NeptunCode);
                return new UserResponse
                {
                    Success = false,
                    Message = $"Failed to create user: {ex.Status.Detail}"
                };
            }
        }

        public override async Task<UserResponse> GetUser(UserNeptunCodeRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Retrieving user with neptun code: {NeptunCode}", request.NeptunCode);

            try
            {
                var databaseRequest = new UserIdRequest
                {
                    NeptunCode = request.NeptunCode
                };

                var databaseResponse = await _databaseUserService.GetUserAsync(databaseRequest);

                return new UserResponse
                {
                    Success = databaseResponse.Success,
                    Message = databaseResponse.Message,
                    User = databaseResponse.User != null ? new UserData
                    {
                        NeptunCode = databaseResponse.User.NeptunCode,
                        Name = databaseResponse.User.Name,
                        Email = databaseResponse.User.Email
                    } : null
                };
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error retrieving user with neptun code: {NeptunCode}", request.NeptunCode);
                return new UserResponse
                {
                    Success = false,
                    Message = $"Failed to retrieve user: {ex.Status.Detail}"
                };
            }
        }

        public override async Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Updating user with neptun code: {NeptunCode}", request.NeptunCode);

            try
            {
                var databaseRequest = new UserRequest
                {
                    NeptunCode = request.NeptunCode,
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password
                };

                var databaseResponse = await _databaseUserService.UpdateUserAsync(databaseRequest);

                return new UserResponse
                {
                    Success = databaseResponse.Success,
                    Message = databaseResponse.Message,
                    User = databaseResponse.User != null ? new UserData
                    {
                        NeptunCode = databaseResponse.User.NeptunCode,
                        Name = databaseResponse.User.Name,
                        Email = databaseResponse.User.Email,
                    } : null
                };
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error updating user with neptun code: {NeptunCode}", request.NeptunCode);
                return new UserResponse
                {
                    Success = false,
                    Message = $"Failed to update user: {ex.Status.Detail}"
                };
            }
        }

        public override async Task<DeleteResponse> DeleteUser(UserNeptunCodeRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Deleting user with neptun code: {NeptunCode}", request.NeptunCode);

            try
            {
                var databaseRequest = new UserIdRequest
                {
                    NeptunCode = request.NeptunCode
                };

                var databaseResponse = await _databaseUserService.DeleteUserAsync(databaseRequest);

                return new DeleteResponse
                {
                    Success = databaseResponse.Success,
                    Message = databaseResponse.Message
                };
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error deleting user with neptun code: {NeptunCode}", request.NeptunCode);
                return new DeleteResponse
                {
                    Success = false,
                    Message = $"Failed to delete user: {ex.Status.Detail}"
                };
            }
        }

        public override async Task<UserListResponse> ListUsers(ListUsersRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Listing all users");

            try
            {

                var databaseRequest = new GetAllUsersRequest();

                var databaseResponse = await _databaseUserService.ListUsersAsync(databaseRequest);

                var response = new UserListResponse();

                foreach (var user in databaseResponse.Users)
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
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error listing users");
                return new UserListResponse();
            }
        }
    }
}