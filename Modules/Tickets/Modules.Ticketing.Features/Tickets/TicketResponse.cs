namespace Modules.Ticketing.Features.Tickets;

/// <summary>
/// Lightweight list projection. Mirrors the old GetTicketDTO conventions
/// (severity/status names, timestamps) adapted to the new domain.
/// </summary>
public sealed record TicketResponse(
	int Id,
	string? Title,
	string Description,
	string Status,
	string? Severity,
	string? Category,
	string? Type,
	string? GroupId,
	DateTime CreatedAt,
	DateTime? UpdatedAt
);
