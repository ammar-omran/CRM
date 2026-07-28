namespace Modules.Ticketing.Features.Ticket.Shared.Responses;

public sealed record TicketResponse(
    int Id,
    string Title,
    string Description,
    string Status,
    string CustomerEmail,
    string CustomerName,
    string CreatedByName,
    DateTime CreatedDate,
    DateTime UpdatedDate
);
