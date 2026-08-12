namespace Modules.Ticketing.Features.Tickets;

public static class TicketRoutes
{
	public const string Base = "api/tickets";
	public const string GetTickets = Base;
	public const string GetTicketById = $"{Base}/{{ticketId}}";
	public const string CreateTicket = Base;
	public const string UpdateTicket = $"{Base}/{{ticketId}}";
	public const string DeleteTicket = $"{Base}/{{ticketId}}";
	public const string AssignTicket = $"{Base}/{{ticketId}}/assign";
	public const string ReplyToTicket = $"{Base}/{{ticketId}}/replies";
}
