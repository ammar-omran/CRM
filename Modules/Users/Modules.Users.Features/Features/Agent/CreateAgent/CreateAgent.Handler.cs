using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Database;
using Modules.Users.Domain.OrganizationAggregate;
using Modules.Users.Features.Agent.Shared.Errors;
using Modules.Users.Features.Agent.Shared.Requests;
using Modules.Users.Features.Agent.Shared.Responses;
using CRM.SharedKernel.Domain.Interfaces;

namespace Modules.Users.Features.Agent.CreateAgent;

internal interface ICreateAgentHandler : IHandler
{
	Task<Result<AgentResponse>> HandleAsync(AddAgentRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateAgentHandler(
		IRepository<Domain.OrganizationAggregate.Agent> agentRepo,
		IReadRepository<Domain.OrganizationAggregate.Agent> agentReadRepo,
		ILogger<CreateAgentHandler> logger) : ICreateAgentHandler
{
	public async Task<Result<AgentResponse>> HandleAsync(
			AddAgentRequest request,
			CancellationToken ct)
	{
		var emailExists = await agentReadRepo.AnyAsync(a => a.Email == request.Email, ct);
		if (emailExists)
		{
			logger.LogInformation("Agent with email {Email} already exists", request.Email);
			return AgentErrors.EmailAlreadyExists(request.Email);
		}

		var agent = new Domain.OrganizationAggregate.Agent
		{
			Name = request.Name,
			Email = request.Email,
		};

		try
		{
			await agentRepo.Add(agent, ct);

			logger.LogInformation("Agent Added with ID {AgentId}", agent.Id);

			return new AgentResponse(
					agent.Id,
					agent.Name,
					agent.Email,
					request.Phone);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An error occurred while adding an Agent with Email {Email}", request.Email);
			return AgentErrors.AddingAgentFailed(request.Email);
		}
	}
}
