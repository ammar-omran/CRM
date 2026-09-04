namespace Modules.Ticketing.Features.Tickets;

public static class TicketRoutes
{
	public const string Base = "api/tickets";
	public const string List = $"{Base}/list";
	public const string Mine = $"{Base}/mine";
	public const string Group = $"{Base}/group";
	public const string GetTicketById = $"{Base}/{{ticketId}}";
	public const string GetTicketHistory = $"{Base}/{{ticketId}}/history";
	public const string CreateTicket = Base;
	public const string LookupsBase = $"{Base}/lookups";
	public const string LookupSeverities = $"{LookupsBase}/severities";
	public const string LookupCategories = $"{LookupsBase}/categories";
	public const string LookupTypes = $"{LookupsBase}/types";
	public const string LookupServices = $"{LookupsBase}/services";
	public const string LookupStatuses = $"{LookupsBase}/statuses";
	public const string LookupTitles = $"{LookupsBase}/titles";
	public const string AssignTicket = $"{Base}/{{ticketId}}/assign";
	public const string ReplyToTicket = $"{Base}/{{ticketId}}/replies";
}
