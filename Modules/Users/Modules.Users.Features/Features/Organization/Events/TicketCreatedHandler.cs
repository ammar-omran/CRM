using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Events;

namespace Modules.Users.Features.Organization.Events;

/// <summary>
/// Stub handler for the <c>ticketing.ticket-created</c> event.
/// Agent assignment is not yet implemented — this only logs receipt.
/// </summary>
internal sealed class TicketCreatedHandler(ILogger<TicketCreatedHandler> logger)
	: IModuleEventHandler<TicketCreatedEvent>
{
	public Task HandleAsync(TicketCreatedEvent @event, CancellationToken cancellationToken = default)
	{
		logger.LogInformation(
			"[Stub] Ticket {TicketId} was created. Agent assignment is not yet implemented.",
			@event.TicketId);
		return Task.CompletedTask;
	}
}
