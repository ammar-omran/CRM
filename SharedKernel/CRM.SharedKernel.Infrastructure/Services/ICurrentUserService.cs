namespace CRM.SharedKernel.Infrastructure.Services;

public sealed record CurrentUser(
	int? UserId,
	string? Name,
	string? Email,
	string? Role,
	string? RawToken,
	IReadOnlyDictionary<string, object>? TokenPayload)
{
	public bool IsAuthenticated => UserId is not null;
}
