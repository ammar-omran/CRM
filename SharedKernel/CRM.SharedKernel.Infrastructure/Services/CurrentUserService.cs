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
				return new CurrentUser(null, null, null, null, null, null);

			var rawToken = GetRawToken(_httpContextAccessor.HttpContext!);

			var userId = principal.FindFirstValue("userid");

			return new CurrentUser(
				userId,
				principal.FindFirstValue(ClaimTypes.Name),
				principal.FindFirstValue(JwtRegisteredClaimNames.Sub),
				principal.FindFirstValue("role"),
				rawToken,
				GetTokenPayload(rawToken));
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

	private static IReadOnlyDictionary<string, object>? GetTokenPayload(string? rawToken)
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
