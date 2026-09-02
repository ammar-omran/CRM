using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Application.API.Responses;
using CRM.SharedKernel.Infrastructure.Database;
using Modules.Users.Features.Agent.Shared.Requests;
using Modules.Users.Features.Agent.Shared.Responses;
using CRM.SharedKernel.Domain.Interfaces;

namespace Modules.Users.Features.Agent.GetAgents;

internal interface IGetAgentsHandler : IHandler
{
	Task<Result<PaginationResponse<AgentResponse>>> HandleAsync(AgentByRequest request, CancellationToken cancellationToken);
}

internal sealed class GetAgentsHandler(
		IReadRepository<Domain.OrganizationAggregate.Agent> agentRepo,
		ILogger<GetAgentsHandler> logger) : IGetAgentsHandler
{
	public async Task<Result<PaginationResponse<AgentResponse>>> HandleAsync(
			AgentByRequest request,
			CancellationToken ct)
	{
		logger.LogInformation("Getting Agents");

		var query = agentRepo.Query;

		if (!string.IsNullOrWhiteSpace(request.UserId))
			query = query.Where(a => a.UserId == request.UserId);

		var total = request.SkipTotal ? -1 : await agentRepo.CountAsync(query, ct);

		query = query
				.Skip(request.Skip)
				.Take(request.Limit);

		var agents = await agentRepo.GetListAsync(
				query: query,
				selector: a => new AgentResponse(a.Id, a.UserId, a.User.UserName ?? string.Empty, a.User.Email ?? string.Empty),
				cancellation: ct);

		var result = new PaginationResponse<AgentResponse>(
				agents,
				request.Skip / request.Limit,
				request.Limit,
				total);

		logger.LogInformation("Retrieved {Count} Agents", result.Items.Count);

		return result;
	}
}
