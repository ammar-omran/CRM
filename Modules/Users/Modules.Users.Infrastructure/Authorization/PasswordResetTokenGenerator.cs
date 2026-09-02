using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CRM.SharedKernel.Infrastructure.Configuration;
using Modules.Users.Domain.Authentication;

namespace Modules.Users.Infrastructure.Authorization;

/// <summary>
/// Stateless password-reset / account-activation token generator. Mirrors the
/// behaviour of the legacy <c>IPasswordResetTokenGenerator</c> (UserId + Expiry
/// carried inside a signed JWT), adapted to this module's string user identifiers
/// and shared <see cref="AuthConfiguration"/>.
/// </summary>
public sealed class PasswordResetTokenGenerator(
    IOptions<AuthConfiguration> authOptions) : IPasswordResetTokenGenerator
{
    public string GenerateToken(string userId, DateTime expires)
    {
        var auth = authOptions.Value;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("UserId", userId),
            new("Expiry", expires.ToBinary().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: auth.Issuer,
            audience: auth.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string UserId, DateTime Expiry)? ValidateToken(string token)
    {
        var auth = authOptions.Value;
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth.Key));

        try
        {
            // Validate signature/issuer/audience but NOT lifetime, so the caller can
            // return a specific "expired" error instead of a generic failure.
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = auth.Issuer,
                ValidAudience = auth.Audience,
                IssuerSigningKey = key,
            }, out _);

            var userIdClaim = principal.FindFirst("UserId")?.Value;
            var expiryClaim = principal.FindFirst("Expiry")?.Value;

            if (userIdClaim is null || expiryClaim is null)
            {
                return null;
            }

            return (userIdClaim, DateTime.FromBinary(long.Parse(expiryClaim)));
        }
        catch
        {
            return null;
        }
    }
}
