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

    public static Error CategoryRequired =>
        Error.Validation("Ticket.CategoryRequired", "A ticket must belong to a category.");

    public static Error TypeRequired =>
        Error.Validation("Ticket.TypeRequired", "A ticket must have a type.");

    public static Error InvalidTitleCategory =>
        Error.Validation("Ticket.InvalidTitleCategory", "The selected title does not belong to the selected category.");

    public static Error TitleRequired =>
        Error.Validation("Ticket.TitleRequired", "A ticket must reference a title or provide an other title.");

    public static Error OperatorRequired =>
        Error.Validation("Ticket.OperatorRequired", "An operator is required to perform this action.");

    public static Error CannotDeleteClosedTicket =>
        Error.Failure("Ticket.CannotDeleteClosedTicket", "Cannot delete a closed ticket.");
}
