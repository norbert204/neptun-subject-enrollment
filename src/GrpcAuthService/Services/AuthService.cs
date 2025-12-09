using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Grpc.Core;
using GrpcAuthService;
using GrpcAuthService.Options;
using GrpcCachingService;
using GrpcDatabaseService.Protos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RefreshTokenResponse = GrpcAuthService.RefreshTokenResponse;

namespace GrpcAuthService.Services;

public class AuthService : GrpcAuthService.AuthService.AuthServiceBase
{
    private readonly AuthDataService.AuthDataServiceClient  _authDataServiceClient;
    private readonly UserService.UserServiceClient _userServiceClient;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        AuthDataService.AuthDataServiceClient authDataServiceClient,
        UserService.UserServiceClient userServiceClient,
        IOptions<JwtOptions> jwtOptions)
    {
        _authDataServiceClient = authDataServiceClient;
        _userServiceClient = userServiceClient;
        _jwtOptions = jwtOptions.Value;
    }

    public override async Task<TokenResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var user = await _userServiceClient.GetUserAsync(
            new UserIdRequest
            {
                NeptunCode = request.NeptunCode,
            });

        if (!user.Success)
        {
            throw new Exception($"User with Neptun code {request.NeptunCode} doesn't exist");
        }

        var encryptor = SHA256.Create();
        
        var passwordBytes = encryptor.ComputeHash(Encoding.ASCII.GetBytes(request.Password));
        var passwordHash = passwordBytes.Aggregate(string.Empty, (current, theByte) => current + theByte.ToString("x2"));

        if (request.NeptunCode != user.User.NeptunCode || passwordHash != user.User.Password)
        {
            throw new Exception("Invalid username or password");
        }

        var (token, refreshToken) = await GetAccessTokenAndRefreshTokenAsync(user.User.Name, user.User.NeptunCode);

        return new TokenResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
        };
    }

    public override async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
    {
        var refreshTokenJwtId = await _authDataServiceClient.GetRefreshTokenFromCacheAsync(
            new GetRefreshTokenRequest
            {
                RefreshToken = request.RefreshToken,
            });

        if (!refreshTokenJwtId.Success)
        {
            throw new Exception("Failed to get refresh token");
        }
        
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = true,
            ValidateLifetime = true,
        };

        var result = await tokenHandler.ValidateTokenAsync(request.AccessToken, tokenValidationParameters);

        if (!result.IsValid)
        {
            throw new Exception("Failed to validate token");
        }

        var validatedToken = result.SecurityToken as JwtSecurityToken;

        if (validatedToken is null)
        {
            throw new Exception("Invalid token");
        }

        // Extract JTI and name/neptun from token claims (use payload, not header)
        var principal = result.Principal;
        var tokenJti = principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var tokenName = principal?.FindFirst(ClaimTypes.Name)?.Value;
        var tokenNeptun = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(tokenJti))
        {
            throw new Exception("Token missing JTI");
        }

        if (tokenJti != refreshTokenJwtId.JwtId)
        {
            throw new Exception("Refresh token doesn't belong to access token");
        }

        await _authDataServiceClient.RemoveRefreshTokenFromCacheAsync(
            new RemoveRefreshTokenRequest
            {
                RefreshToken = request.RefreshToken,
            });

        var (token, refreshToken) = await GetAccessTokenAndRefreshTokenAsync(
            tokenName ?? string.Empty,
            tokenNeptun ?? string.Empty);

        return new RefreshTokenResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
        };
    }

    private async Task<(string, string)> GetAccessTokenAndRefreshTokenAsync(string name, string neptunCode)
    {
        var jti = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jti),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.NameIdentifier, neptunCode),
            new(ClaimTypes.Role, "student"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        var tokenDescriptor = new JwtSecurityToken(
            _jwtOptions.Issuer,
            signingCredentials: new SigningCredentials(key,  SecurityAlgorithms.HmacSha256),
            claims: claims,
            expires: DateTime.UtcNow.Add(_jwtOptions.AccessTokenLifetime));
        
        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        var refreshToken = Guid.NewGuid().ToString();

        var cacheResult = await _authDataServiceClient.AddRefreshTokenToCacheAsync(
            new RefreshTokenRegistrationRequest
            {
                JwtId = jti,
                RefreshToken = refreshToken,
                RefreshTokenLifetime = _jwtOptions.RefreshTokenLifetime,
            });

        if (!cacheResult.Success)
        {
            throw new Exception("Failed to add refresh token");
        }

        return (token, refreshToken);
    }
}