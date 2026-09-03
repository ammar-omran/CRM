using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Application.API.Responses;
using Modules.Users.Features.Agents.Shared.Requests;
using Modules.Users.Features.Agents.Shared.Responses;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Agents.GetAgents;

public interface IGetAgentsHandler : IHandler
{
	Task<Result<PaginationResponse<AgentResponse>>> HandleAsync(AgentByRequest request, CancellationToken cancellationToken);
}

internal sealed class GetAgentsHandler(
		OrganizationsDbContext dbContext,
		ILogger<GetAgentsHandler> logger) : IGetAgentsHandler
{
	public async Task<Result<PaginationResponse<AgentResponse>>> HandleAsync(
			AgentByRequest request,
			CancellationToken ct)
	{
		logger.LogInformation("Getting Agents");

		var query = dbContext.Agents.AsQueryable();

		if (!string.IsNullOrWhiteSpace(request.UserId))
			query = query.Where(a => a.UserId == request.UserId);

		var total = request.SkipTotal ? -1 : await query.CountAsync(ct);

		var agents = await query
				.OrderBy(a => a.Id)
				.Skip(request.Skip)
				.Take(request.Limit)
				.Select(a => new AgentResponse(a.Id, a.UserId, a.User.UserName ?? string.Empty, a.User.Email ?? string.Empty))
				.ToListAsync(ct);

		var result = new PaginationResponse<AgentResponse>(
				agents,
				request.Skip / request.Limit,
				request.Limit,
				total);

		logger.LogInformation("Retrieved {Count} Agents", result.Items.Count);

		return result;
	}
}
