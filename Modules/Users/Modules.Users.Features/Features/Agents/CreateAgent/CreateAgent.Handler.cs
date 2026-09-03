using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Users.Domain.OrganizationAggregate;
using Modules.Users.Domain.UserAggregate;
using Modules.Users.Features.Agents.Shared.Errors;
using Modules.Users.Features.Agents.Shared.Requests;
using Modules.Users.Features.Agents.Shared.Responses;
using CRM.SharedKernel.Domain.Interfaces;

namespace Modules.Users.Features.Agents.CreateAgent;

internal interface ICreateAgentHandler : IHandler
{
	Task<Result<AgentResponse>> HandleAsync(AddAgentRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateAgentHandler(
		IRepository<Agent> agentRepo,
		IReadRepository<Agent> agentReadRepo,
		UserManager<User> userManager,
		ILogger<CreateAgentHandler> logger) : ICreateAgentHandler
{
	public async Task<Result<AgentResponse>> HandleAsync(
			AddAgentRequest request,
			CancellationToken ct)
	{
		var user = await userManager.FindByIdAsync(request.UserId);
		if (user is null)
		{
			logger.LogInformation("User with ID {UserId} not found", request.UserId);
			return AgentErrors.UserNotFound(request.UserId);
		}

		var alreadyAgent = await agentReadRepo.AnyAsync(a => a.UserId == request.UserId, ct);
		if (alreadyAgent)
		{
			logger.LogInformation("Agent for user {UserId} already exists", request.UserId);
			return AgentErrors.UserAlreadyAgent(request.UserId);
		}

		var agent = new Agent
		{
			UserId = request.UserId,
		};

		try
		{
			await agentRepo.Add(agent, ct);

			logger.LogInformation("Agent Added with ID {AgentId} for User {UserId}", agent.Id, request.UserId);

			return new AgentResponse(
					agent.Id,
					request.UserId,
					user.UserName ?? user.Email ?? string.Empty,
					user.Email ?? string.Empty);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An error occurred while adding an Agent for User {UserId}", request.UserId);
			return AgentErrors.AddingAgentFailed(request.UserId);
		}
	}
}
