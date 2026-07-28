using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CRM.SharedKernel.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CRM.SharedKernel.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
	private readonly AuthConfiguration _authConfig;

	public JwtTokenGenerator(IOptions<AuthConfiguration> authConfig)
	{
		_authConfig = authConfig.Value;
	}

	public string GenerateToken(string userId, string email, IEnumerable<Claim>? additionalClaims = null)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, userId),
			new(ClaimTypes.Email, email),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		if (additionalClaims is not null)
		{
			claims.AddRange(additionalClaims);
		}

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authConfig.Key));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _authConfig.Issuer,
			audience: _authConfig.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddHours(1),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken()
	{
		var randomNumber = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomNumber);
		return Convert.ToBase64String(randomNumber);
	}
}

