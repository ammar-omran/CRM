using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Configuration;
using Modules.Users.Domain.Authentication;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Tokens;
using Modules.Users.Domain.UserAggregate;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Authorization;

public class ClientAuthorizationService(
		UserManager<User> userManager,
		SignInManager<User> signInManager,
		RoleManager<Role> roleManager,
		ILogger<ClientAuthorizationService> logger,
		IOptions<AuthConfiguration> authOptions,
		TokenValidationParameters tokenValidationParameters,
		UsersDbContext dbContext,
		IMemoryCache memoryCache)
		: IClientAuthorizationService
{
	public async Task<Result<LoginUserResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken)
	{
		var user = await userManager.FindByEmailAsync(email);
		if (user is null)
		{
			return UserErrors.NotFoundByEmail(email);
		}

		var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
		if (!result.Succeeded)
		{
			return UserErrors.InvalidCredentials();
		}

		var (token, refreshToken) = await GenerateJwtAndRefreshTokenAsync(user, null);

		return new LoginUserResponse(token, refreshToken);
	}

	public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken)
	{
		var validatedToken = GetPrincipalFromToken(token, tokenValidationParameters);
		if (validatedToken is null)
		{
			return UserErrors.InvalidToken();
		}

		var jti = validatedToken.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
		if (string.IsNullOrEmpty(jti))
		{
			return UserErrors.InvalidToken();
		}

		var storedRefreshToken = await dbContext.Set<RefreshToken>().FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);
		if (storedRefreshToken is null)
		{
			logger.LogWarning("Refresh token does not exist");
			return UserErrors.InvalidToken();
		}

		if (DateTime.Now > storedRefreshToken.ExpiryDate)
		{
			logger.LogWarning("Refresh token has expired");
			return UserErrors.InvalidToken();
		}

		if (storedRefreshToken.Invalidated)
		{
			logger.LogWarning("Refresh token has been invalidated");
			return UserErrors.InvalidToken();
		}

		if (storedRefreshToken.JwtId != jti)
		{
			logger.LogWarning("Refresh token does not match this JWT");
			return UserErrors.InvalidToken();
		}

		var userId = validatedToken.Claims.FirstOrDefault(x => x.Type == "userid")?.Value;
		if (userId is null)
		{
			logger.LogWarning("Current user is not found");
			return UserErrors.InvalidToken();
		}

		var user = await userManager.FindByIdAsync(userId);
		if (user is null)
		{
			logger.LogWarning("Current user is not found");
			return UserErrors.InvalidToken();
		}

		var (newToken, newRefreshToken) = await GenerateJwtAndRefreshTokenAsync(user, refreshToken);
		return new RefreshTokenResponse(newToken, newRefreshToken);
	}

	private async Task<(string token, string refreshToken)> GenerateJwtAndRefreshTokenAsync(User user, string? existingRefreshToken)
	{
		var roleNames = await userManager.GetRolesAsync(user);
		if (roleNames.Count == 0)
		{
			roleNames = new List<string> { "user" };
		}

		// Collect the claims of EVERY assigned role, walking up each role's ParentRole chain so that
		// claims inherited from parent roles are included as well. Claims are de-duplicated by
		// (Type, Value) so a user with several roles does not end up with duplicate entries.
		var allClaims = new List<Claim>();
		var seenClaims = new HashSet<string>(StringComparer.Ordinal);
		var visitedRoleIds = new HashSet<string>(StringComparer.Ordinal);

		foreach (var roleName in roleNames)
		{
			var current = await roleManager.FindByNameAsync(roleName);
			while (current is not null && visitedRoleIds.Add(current.Id))
			{
				var claims = await roleManager.GetClaimsAsync(current);
				foreach (var claim in claims)
				{
					var key = $"{claim.Type}{(char)0x1F}{claim.Value}";
					if (seenClaims.Add(key))
					{
						allClaims.Add(claim);
					}
				}

				current = current.PairentRoleId is not null
					? await roleManager.FindByIdAsync(current.PairentRoleId)
					: null;
			}
		}

		var token = GenerateJwtToken(user, authOptions.Value, roleNames, allClaims);
		var refreshToken = await GenerateRefreshTokenAsync(token, user, existingRefreshToken);

		return (token, refreshToken);
	}

	private async Task<string> GenerateRefreshTokenAsync(string token, User user, string? existingRefreshToken)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var jwtToken = tokenHandler.ReadJwtToken(token);
		var jti = jwtToken.Id;

		var refreshToken = new RefreshToken
		{
			Token = Guid.NewGuid().ToString(),
			JwtId = jti,
			UserId = user.Id,
			ExpiryDate = DateTime.Now.AddDays(7),
			CreatedAt = DateTime.Now,
		};

		if (!string.IsNullOrEmpty(existingRefreshToken))
		{
			var existingToken = await dbContext.Set<RefreshToken>().FirstOrDefaultAsync(x => x.Token == existingRefreshToken);
			if (existingToken != null)
			{
				dbContext.Set<RefreshToken>().Remove(existingToken);
			}
		}

		await dbContext.AddAsync(refreshToken);
		await dbContext.SaveChangesAsync();

		return refreshToken.Token;
	}

	private static string GenerateJwtToken(User user,
			AuthConfiguration authConfiguration,
			IEnumerable<string> roles,
			IList<Claim> roleClaims)
	{
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfiguration.Key));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

		var tokenId = Guid.NewGuid().ToString();
		List<Claim> claims = [
				new(JwtRegisteredClaimNames.Sub, user.Email!),
						new("userid", user.Id), // TODO: Stander name
						new(JwtRegisteredClaimNames.Jti, tokenId)
		];

		foreach (var role in roles)
		{
			claims.Add(new Claim("role", role));
		}

		foreach (var roleClaim in roleClaims)
		{
			claims.Add(new Claim(roleClaim.Type, roleClaim.Value));
		}

		var token = new JwtSecurityToken(
				issuer: authConfiguration.Issuer,
				audience: authConfiguration.Audience,
				claims: claims,
				expires: DateTime.Now.AddMinutes(30),
				signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	private static ClaimsPrincipal? GetPrincipalFromToken(string token, TokenValidationParameters parameters)
	{
		var tokenHandler = new JwtSecurityTokenHandler();

		try
		{
			var tokenValidationParameters = parameters.Clone();

#pragma warning disable CA5404
			tokenValidationParameters.ValidateLifetime = false;
#pragma warning restore CA5404

			var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
			return IsJwtWithValidSecurityAlgorithm(validatedToken) ? principal : null;
		}
		catch
		{
			return null;
		}
	}

	private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
			=> validatedToken is JwtSecurityToken jwtSecurityToken
				 && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

	/// <summary>
	/// Updates a user's role and invalidates their refresh tokens
	/// </summary>
	/// <param name="userId">The ID of the user to update</param>
	/// <param name="newRole">The new role to assign to the user</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>A result containing success or failure information</returns>
	public async Task<Result<Success>> UpdateUserRoleAsync(string userId, string newRole, CancellationToken cancellationToken)
	{
		// Verify the role exists
		var role = await roleManager.FindByNameAsync(newRole);
		if (role is null)
		{
			logger.LogWarning("Role '{NewRole}' does not exist", newRole);
			return UserErrors.RoleNotFound(newRole);
		}

		// Find the user
		var user = await userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return UserErrors.NotFound(userId);
		}

		// Get current roles and remove them
		var currentRoles = await userManager.GetRolesAsync(user);
		if (currentRoles.Any())
		{
			await userManager.RemoveFromRolesAsync(user, currentRoles);
		}

		// Add the new role
		var addRoleResult = await userManager.AddToRoleAsync(user, newRole);
		if (!addRoleResult.Succeeded)
		{
			logger.LogError("Failed to add role '{NewRole}' to user '{UserId}': {@Errors}", newRole, userId, addRoleResult.Errors);
			return UserErrors.UpdateRoleFailed(addRoleResult.Errors);
		}

		// Invalidate all refresh tokens for this user
		var refreshTokens = await dbContext.RefreshTokens
				.Where(rt => rt.UserId == userId && !rt.Invalidated)
				.ToListAsync(cancellationToken);

		foreach (var refreshToken in refreshTokens)
		{
			refreshToken.Invalidated = true;
			refreshToken.UpdatedAt = DateTime.Now;

			// Add to memory cache for the middleware to check
			memoryCache.Set(refreshToken.JwtId, RevocatedTokenType.RoleChanged);
		}

		await dbContext.SaveChangesAsync(cancellationToken);

		return Result.Success;
	}
}
