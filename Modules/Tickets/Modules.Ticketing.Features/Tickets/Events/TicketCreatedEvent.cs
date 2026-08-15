using CRM.SharedKernel.Domain.Events;

namespace Modules.Ticketing.Features.Tickets.Events;

[ModuleEvent("ticketing.ticket-created")]
public sealed record TicketCreatedEvent(int TicketId) : IModuleEvent;
