using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Events;

namespace Modules.Users.Features.Organizations.Events;

/// <summary>
/// Stub handler for the <c>ticketing.ticket-created</c> event.
/// Agent assignment is not yet implemented — this only logs receipt.
/// </summary>
internal sealed class TicketCreatedHandler(ILogger<TicketCreatedHandler> logger)
	: ModuleEventHandler<TicketCreatedEvent>
{
	protected override Task HandleAsync(TicketCreatedEvent @event, CancellationToken cancellationToken = default)
	{
		logger.LogInformation(
			"[Stub] Ticket {TicketId} was created. Agent assignment is not yet implemented.",
			@event.TicketId);
		return Task.CompletedTask;
	}
}
