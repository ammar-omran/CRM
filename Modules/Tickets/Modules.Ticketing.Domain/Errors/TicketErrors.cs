using CRM.SharedKernel.Domain.Results;

namespace Modules.Ticketing.Domain.Errors;

public static class TicketErrors
{
    public static Error NotFound(string ticketId) =>
        Error.NotFound("Ticket.NotFound", $"Ticket with ID {ticketId} was not found.");

    public static Error NotFound(int ticketDbId) =>
        Error.NotFound("Ticket.NotFound", $"Ticket with database ID {ticketDbId} was not found.");

    public static Error UnauthorizedAccess =>
        Error.Forbidden("Ticket.UnauthorizedAccess", "You do not have access to this ticket.");

    public static Error AlreadyAssigned =>
        Error.Conflict("Ticket.AlreadyAssigned", "This ticket is already assigned.");

    public static Error InvalidStatusTransition =>
        Error.Failure("Ticket.InvalidStatusTransition", "The requested status transition is not allowed.");

    public static Error CannotDeleteClosedTicket =>
        Error.Failure("Ticket.CannotDeleteClosedTicket", "Cannot delete a closed ticket.");
}
