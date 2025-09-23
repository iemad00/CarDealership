using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using CarDealership.Models;
using StackExchange.Redis;
using CarDealership.Options;
using Microsoft.Extensions.Options;

namespace CarDealership.Services;

public class JwtService : IJwtService
{
    private const string PhoneClaimType = "phone";
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<JwtService> _logger;
    private readonly IDatabase _redis;

    public JwtService(IOptions<JwtOptions> jwtOptions, ILogger<JwtService> logger, IConnectionMultiplexer redis)
    {
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    public string GenerateLoginToken(string phone)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("type", "login"),
            new Claim(PhoneClaimType, phone),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10), // 10 minutes
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateAccessToken(Models.User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("type", "access"),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Phone),
            new Claim(PhoneClaimType, user.Phone),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15), // 15 minutes
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateAccessToken(AdminUser adminUser)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("type", "access"),
            new Claim(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
            new Claim(ClaimTypes.Name, adminUser.Phone),
            new Claim(PhoneClaimType, adminUser.Phone),
            new Claim("userType", "AdminUser"),
            new Claim("name", adminUser.Name),
            new Claim("email", adminUser.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15), // 15 minutes
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(Models.User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("type", "refresh"),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(PhoneClaimType, user.Phone),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30), // 30 days
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(AdminUser adminUser)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("type", "refresh"),
            new Claim(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
            new Claim(PhoneClaimType, adminUser.Phone),
            new Claim("userType", "AdminUser")
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30), // 30 days
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtOptions.Key);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return false;
        }
    }

    public int? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract user ID from token");
        }
        
        return null;
    }

    public string? GetPhoneFromLoginToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var typeClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "type");
            var phoneClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == PhoneClaimType);
            
            if (typeClaim?.Value == "login" && phoneClaim != null)
            {
                return phoneClaim.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract phone from login token");
        }
        
        return null;
    }

    public bool ValidateRefreshToken(string token, int userId)
    {
        try
        {
            // Check if token is invalidated in Redis
            var invalidatedKey = $"invalidated_refresh_token:{token}";
            var isInvalidated = _redis.KeyExists(invalidatedKey);
            if (isInvalidated)
            {
                _logger.LogWarning("Refresh token has been invalidated");
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var typeClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "type");
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            
            return typeClaim?.Value == "refresh" && 
                   userIdClaim != null && 
                   int.TryParse(userIdClaim.Value, out int tokenUserId) && 
                   tokenUserId == userId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate refresh token");
            return false;
        }
    }

    public async Task<bool> InvalidateRefreshTokenAsync(string token)
    {
        try
        {
            // Store token in Redis as invalidated with 30 days expiration (same as refresh token lifetime)
            var invalidatedKey = $"invalidated_refresh_token:{token}";
            await _redis.StringSetAsync(invalidatedKey, "invalidated", TimeSpan.FromDays(30));
            
            _logger.LogInformation("Refresh token invalidated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invalidate refresh token");
            return false;
        }
    }
}
