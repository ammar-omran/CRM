using CRM.SharedKernel.Domain.Events;

namespace Modules.Users.Features.Organizations.Events;

[ModuleEvent("ticketing.ticket-created")]
public sealed record TicketCreatedEvent(int TicketId) : IModuleEvent;
