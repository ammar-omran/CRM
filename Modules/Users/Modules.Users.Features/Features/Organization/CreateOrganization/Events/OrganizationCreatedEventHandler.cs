using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.SharedKernel.Domain.Events;
using CRM.SharedKernel.Infrastructure.Database;
using Modules.Users.Domain.OrganizationAggregate;
using CRM.SharedKernel.Infrastructure.Services;
using CRM.SharedKernel.Infrastructure.Configuration;
using CRM.SharedKernel.Domain.Interfaces;

namespace Modules.Users.Features.Organization.CreateOrganization.Events;

internal sealed class OrganizationCreatedEventHandler(
	IReadRepository<OrganizationAgent> orgAgentRepo,
	IEmailSender emailSender,
	IOptions<EmailOptions> emailOptions,
	ILogger<OrganizationCreatedEventHandler> logger)
	: IEventHandler<OrganizationCreatedEvent>
{
	public async Task HandleAsync(OrganizationCreatedEvent @event, CancellationToken ct)
	{
		logger.LogInformation(
			"Sending email notifications for Organization {OrgId} ({OrgName})",
			@event.OrganizationId,
			@event.OrganizationName);

		var orgAgents = await orgAgentRepo.GetListAsync(
			query: orgAgentRepo.Query.Where(oa => oa.OrganizationId == @event.OrganizationId),
			includes: [nameof(OrganizationAgent.Agent), nameof(OrganizationAgent.AgentRole)],
			cancellation: ct);

		if (orgAgents.Count == 0)
		{
			logger.LogInformation("No agents assigned to Organization {OrgId}", @event.OrganizationId);
			return;
		}

		var cfg = emailOptions.Value;

		foreach (var orgAgent in orgAgents)
		{
			var body = OrganizationTemplates.AgentAddedToOrganizationEmailContent(
				agentName: orgAgent.Agent.Name,
				agentEmail: orgAgent.Agent.Email,
				tempPassword: "",
				OrgName: @event.OrganizationName,
				agentRole: orgAgent.AgentRole.Name ?? string.Empty,
				systemURL: cfg.SystemUrl);

			await emailSender.SendAsync(
				to: orgAgent.Agent.Email,
				subject: $"You have been added to {@event.OrganizationName}",
				htmlBody: body,
				ct: ct);
		}

		logger.LogInformation(
			"Sent {Count} email notifications for Organization {OrgId}",
			orgAgents.Count,
			@event.OrganizationId);
	}
}
