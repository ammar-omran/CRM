using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace CRM.SharedKernel.Infrastructure.Services;

public interface ICurrentUserService
{
	CurrentUser CurrentUser { get; }
}

public sealed class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public CurrentUser CurrentUser
	{
		get
		{
			var principal = _httpContextAccessor.HttpContext?.User;
			if (principal is null)
				return new CurrentUser(null, null, null, null, null, Array.Empty<string>());

			var rawToken = GetRawToken(_httpContextAccessor.HttpContext!);

			var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
				?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
				?? principal.FindFirstValue("userid");

			// Collect all role claims (ClaimTypes.Role + "role"), deduplicated, trimmed.
			var roles = principal.FindAll(ClaimTypes.Role)
				.Select(c => c.Value)
				.Concat(principal.FindAll("role").Select(c => c.Value))
				.Where(v => !string.IsNullOrWhiteSpace(v))
				.Select(v => v.Trim())
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			return new CurrentUser(
				userId,
				principal.FindFirstValue(ClaimTypes.Name),
				principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Email) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub),
				rawToken,
				GetTokenPayload(rawToken),
				roles);
		}
	}

	private static string? GetRawToken(HttpContext httpContext)
	{
		var header = httpContext.Request.Headers[HeaderNames.Authorization].FirstOrDefault();
		if (string.IsNullOrWhiteSpace(header))
			return null;

		const string scheme = "Bearer ";
		return header.StartsWith(scheme, StringComparison.OrdinalIgnoreCase)
			? header[scheme.Length..].Trim()
			: null;
	}

	private static System.IdentityModel.Tokens.Jwt.JwtPayload? GetTokenPayload(string? rawToken)
	{
		if (string.IsNullOrEmpty(rawToken))
			return null;

		try
		{
			var handler = new JwtSecurityTokenHandler();
			return handler.CanReadToken(rawToken)
				? handler.ReadJwtToken(rawToken).Payload
				: null;
		}
		catch (Exception)
		{
			return null; // malformed token — don't crash the request
		}
	}

}
