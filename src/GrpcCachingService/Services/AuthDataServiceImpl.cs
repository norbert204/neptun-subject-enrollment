using Grpc.Core;
using GrpcCachingService.Repositories.Interfaces;

namespace GrpcCachingService.Services;

public class AuthDataServiceImpl : AuthDataService.AuthDataServiceBase
{
    private IAuthRepository _authRepository;
    
    public AuthDataServiceImpl(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }
    
    public override async Task<GenericResponse> AddRefreshTokenToCache(RefreshTokenRegistrationRequest request, ServerCallContext context)
    {
        var result = await _authRepository.StoreRefreshTokenAsync(
            request.JwtId,
            request.RefreshToken,
            request.RefreshTokenLifetime);

        if (!result)
        {
            return new GenericResponse
            {
                Message = "Refresh token could not be saved.",
                Success = false,
            };
        }

        return new GenericResponse
        {
            Message = "Refresh token saved.",
            Success = true,
        };
    }

    public override async Task<RefreshTokenResponse> GetRefreshTokenFromCache(GetRefreshTokenRequest request, ServerCallContext context)
    {
        var jwtId = await _authRepository.GetJwtIdForRefreshTokenAsync(request.RefreshToken);

        if (jwtId is null)
        {
            return new RefreshTokenResponse
            {
                Success = false,
            };
        }

        return new RefreshTokenResponse
        {
            JwtId = jwtId,
            Success = true,
        };
    }

    public override async Task<GenericResponse> RemoveRefreshTokenFromCache(RemoveRefreshTokenRequest request, ServerCallContext context)
    {
        await _authRepository.RemoveRefreshTokenAsync(request.RefreshToken);

        return new GenericResponse
        {
            Message = "Refresh token removed.",
            Success = true,
        };
    }
}