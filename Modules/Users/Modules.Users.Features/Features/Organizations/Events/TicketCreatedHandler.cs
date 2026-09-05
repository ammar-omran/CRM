using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.SharedKernel.Domain.Events;
using CRM.SharedKernel.Infrastructure.Configuration;
using CRM.SharedKernel.Infrastructure.Services;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Organizations.Events;

internal sealed class TicketCreatedHandler(
	OrganizationsDbContext dbContext,
	IEmailSender emailSender,
	IOptions<EmailOptions> emailOptions,
	ILogger<TicketCreatedHandler> logger)
	: ModuleEventHandler<TicketCreatedEvent>
{
	protected override async Task HandleAsync(TicketCreatedEvent @event, CancellationToken cancellationToken = default)
	{
		logger.LogInformation(
			"Received ticket-created event: Ticket {TicketId}, Group {GroupId}, Title {Title}",
			@event.TicketId, @event.GroupId, @event.Title);

		if (string.IsNullOrWhiteSpace(@event.GroupId))
		{
			logger.LogWarning("Ticket {TicketId} has no GroupId — skipping organization notification", @event.TicketId);
			return;
		}

		if (!int.TryParse(@event.GroupId, out var organizationId))
		{
			logger.LogWarning("Ticket {TicketId} GroupId '{GroupId}' is not a valid organization id", @event.TicketId, @event.GroupId);
			return;
		}

		var organization = await dbContext.Organizations
			.AsNoTracking()
			.Include(o => o.OrganizationAgents)
				.ThenInclude(oa => oa.Agent)
					.ThenInclude(a => a.User)
			.Include(o => o.OrganizationAgents)
				.ThenInclude(oa => oa.AgentRole)
			.Where(o => o.Id == organizationId)
			.FirstOrDefaultAsync(cancellationToken);

		if (organization is null)
		{
			logger.LogWarning("Organization {OrganizationId} not found for ticket {TicketId}", organizationId, @event.TicketId);
			return;
		}

		var agents = organization.OrganizationAgents;
		if (agents.Count == 0)
		{
			logger.LogInformation("Organization {OrganizationId} has no agents — nothing to notify for ticket {TicketId}", organizationId, @event.TicketId);
			return;
		}

		var cfg = emailOptions.Value;
		var systemUrl = string.IsNullOrWhiteSpace(cfg.SystemUrl) ? "http://localhost:5000" : cfg.SystemUrl;

		foreach (var orgAgent in agents)
		{
			var user = orgAgent.Agent.User;
			var agentEmail = user.Email;
			if (string.IsNullOrWhiteSpace(agentEmail))
			{
				logger.LogWarning("Agent {AgentId} has no email — skipping notification for ticket {TicketId}", orgAgent.AgentId, @event.TicketId);
				continue;
			}

			var agentName = user.UserName ?? agentEmail;
			var roleName = orgAgent.AgentRole?.Name ?? orgAgent.AgentRoleId;

			var body = TicketNotificationTemplates.NewTicketEmail(
				agentName: agentName,
				organizationName: organization.Name,
				ticketId: @event.TicketId,
				ticketTitle: @event.Title ?? "(no title)",
				ticketDescription: @event.Description ?? string.Empty,
				agentRole: roleName,
				systemUrl: systemUrl);

			try
			{
				await emailSender.SendAsync(
					to: agentEmail,
					subject: $"New ticket #{@event.TicketId} — {organization.Name}",
					htmlBody: body,
					ct: cancellationToken);

				logger.LogInformation("Notified agent {AgentEmail} about ticket {TicketId}", agentEmail, @event.TicketId);
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "Failed to notify agent {AgentEmail} about ticket {TicketId}", agentEmail, @event.TicketId);
			}
		}

		logger.LogInformation(
			"Completed ticket-created workflow for ticket {TicketId}: notified {Count} agent(s) in organization {OrganizationId}",
			@event.TicketId, agents.Count, organizationId);

		// Explicit assignment is deferred to POST /api/tickets/{id}/assign (AssignPolicy).
		// Auto-assignment could be added here via an HTTP call to the Ticketing module
		// using IHttpClientFactory + ServiceUrls:TicketingApi, but is intentionally
		// omitted to keep the modules loosely coupled — the notification above is the
		// actionable signal for agents/supervisors.
	}
}
