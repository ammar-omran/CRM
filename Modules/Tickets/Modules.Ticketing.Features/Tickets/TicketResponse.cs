namespace Modules.Ticketing.Features.Tickets;

public sealed record TicketResponse(
	int Id,
	string? Title,
	string Description,
	string Status,
	DateTime CreatedAt,
	DateTime? UpdatedAt
);
