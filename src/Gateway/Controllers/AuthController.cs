using System.Net;
using Gateway.DTOs.Auth;
using GrpcAuthService;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = Gateway.DTOs.Auth.LoginRequest;
using RefreshTokenRequest = Gateway.DTOs.Auth.RefreshTokenRequest;
using RefreshTokenResponse = Gateway.DTOs.Auth.RefreshTokenResponse;

namespace Gateway.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly AuthService.AuthServiceClient _authServiceClient;

    public AuthController(ILogger<AuthController> logger, AuthService.AuthServiceClient authDataServiceClient)
    {
        _logger = logger;
        _authServiceClient = authDataServiceClient;
    }

    [HttpPost]
    public async Task<ActionResult<LoginResponse>> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken)
    {
        var request = new GrpcAuthService.LoginRequest
        {
            NeptunCode = loginRequest.NeptunCode,
            Password = loginRequest.Password,
        };

        try
        {
            var response = await _authServiceClient.LoginAsync(request, cancellationToken: cancellationToken);

            return Ok(
                new LoginResponse
                {
                    AccessToken = response.AccessToken,
                    RefreshToken = response.RefreshToken
                });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Login endpoint returned error");
            
            return StatusCode(500, new ProblemDetails
            {
                Detail = ex.Message,
                Status = (int)HttpStatusCode.InternalServerError,
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken)
    {
        var request = new GrpcAuthService.RefreshTokenRequest
        {
            AccessToken = refreshTokenRequest.AccessToken,
            RefreshToken = refreshTokenRequest.RefreshToken,
        };
        
        var result = await _authServiceClient.RefreshTokenAsync(request, cancellationToken: cancellationToken);

        if (result.Success)
        {
            return Ok(
                new RefreshTokenResponse
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                });
        }

        return BadRequest(
            new ProblemDetails
            {
                Detail = "Refresh token returned error",
                Status = (int)HttpStatusCode.BadRequest,
            });
    }
}