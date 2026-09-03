using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.SharedKernel.Domain.Events;
using Modules.Users.Infrastructure.Database;
using CRM.SharedKernel.Infrastructure.Services;
using CRM.SharedKernel.Infrastructure.Configuration;

namespace Modules.Users.Features.Organizations.CreateOrganization.Events;

internal sealed class OrganizationCreatedEventHandler(
	OrganizationsDbContext dbContext,
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

		var orgAgents = await dbContext.OrganizationAgents
			.Include(oa => oa.Agent)
				.ThenInclude(a => a.User)
			.Include(oa => oa.AgentRole)
			.Where(oa => oa.OrganizationId == @event.OrganizationId)
			.ToListAsync(ct);

		if (orgAgents.Count == 0)
		{
			logger.LogInformation("No agents assigned to Organization {OrgId}", @event.OrganizationId);
			return;
		}

		var cfg = emailOptions.Value;

		foreach (var orgAgent in orgAgents)
		{
			var user = orgAgent.Agent.User;
			var agentName = user.UserName ?? user.Email ?? string.Empty;
			var agentEmail = user.Email ?? string.Empty;
			var body = OrganizationTemplates.AgentAddedToOrganizationEmailContent(
				agentName: agentName,
				agentEmail: agentEmail,
				tempPassword: "",
				OrgName: @event.OrganizationName,
				agentRole: orgAgent.AgentRole.Name ?? string.Empty,
				systemURL: cfg.SystemUrl);

			await emailSender.SendAsync(
				to: agentEmail,
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
