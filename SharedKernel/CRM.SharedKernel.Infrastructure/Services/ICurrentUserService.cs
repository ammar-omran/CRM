namespace CRM.SharedKernel.Infrastructure.Services;

public sealed record CurrentUser(
	string? UserId,
	string? Name,
	string? Email,
	string? RawToken,
	IReadOnlyDictionary<string, object>? TokenPayload,
	IReadOnlyList<string> Roles)
{
	/// <summary>Legacy single-role accessor (first role). Prefer <see cref="Roles"/>.</summary>
	public string? Role => Roles.Count > 0 ? Roles[0] : null;

	/// <summary>Roles joined with ';' — the on-wire snapshot format used in TicketOperator.</summary>
	public string RolesCsv => string.Join(';', Roles);

	public bool IsAuthenticated => UserId is not null;

	/// <summary>Backward-compat ctor — single Role.</summary>
	public CurrentUser(string? UserId, string? Name, string? Email, string? Role, string? RawToken, IReadOnlyDictionary<string, object>? TokenPayload)
		: this(UserId, Name, Email, RawToken, TokenPayload, Role is not null ? new[] { Role } : Array.Empty<string>())
	{ }
}
