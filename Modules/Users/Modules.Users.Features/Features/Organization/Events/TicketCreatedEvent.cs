using CRM.SharedKernel.Domain.Events;

namespace Modules.Users.Features.Organization.Events;

[ModuleEvent("ticketing.ticket-created")]
public sealed record TicketCreatedEvent(int TicketId) : IModuleEvent;
