using CRM.SharedKernel.Domain.Events;

namespace Modules.Ticketing.Features.Tickets.Events;

[ModuleEvent("ticketing.ticket-created")]
public sealed record TicketCreatedEvent(
	int TicketId,
	string? GroupId = null,
	string? Title = null,
	string? Description = null) : IModuleEvent;
