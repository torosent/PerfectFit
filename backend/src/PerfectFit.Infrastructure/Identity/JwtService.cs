using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PerfectFit.Infrastructure.Identity;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly TokenValidationParameters _validationParameters;

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;

        var key = Encoding.UTF8.GetBytes(_settings.Secret);
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("external_id", user.ExternalId),
            new("provider", user.Provider.ToString())
        };

        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        if (!string.IsNullOrEmpty(user.Avatar))
        {
            claims.Add(new Claim("avatar", user.Avatar));
        }

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_settings.ExpirationDays),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        return ValidateToken(token, true);
    }

    public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters(validateLifetime);
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return principal;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private TokenValidationParameters GetValidationParameters(bool validateLifetime)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = _validationParameters.ValidateIssuerSigningKey,
            IssuerSigningKey = _validationParameters.IssuerSigningKey,
            ValidateIssuer = _validationParameters.ValidateIssuer,
            ValidIssuer = _validationParameters.ValidIssuer,
            ValidateAudience = _validationParameters.ValidateAudience,
            ValidAudience = _validationParameters.ValidAudience,
            ValidateLifetime = validateLifetime,
            ClockSkew = _validationParameters.ClockSkew
        };
    }
}
