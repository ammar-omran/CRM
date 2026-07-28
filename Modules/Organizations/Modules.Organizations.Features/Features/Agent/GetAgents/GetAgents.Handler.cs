using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.API.Responses;
using CRM.SharedKernel.Infrastructure.Database;
using Modules.Organizations.Features.Agent.Shared.Requests;
using Modules.Organizations.Features.Agent.Shared.Responses;

namespace Modules.Organizations.Features.Agent.GetAgents;

internal interface IGetAgentsHandler : IHandler
{
    Task<Result<PaginationResponse<AgentResponse>>> HandleAsync(AgentByRequest request, CancellationToken cancellationToken);
}

internal sealed class GetAgentsHandler(
    IReadRepository<Domain.Entities.Agent> agentRepo,
    ILogger<GetAgentsHandler> logger) : IGetAgentsHandler
{
    public async Task<Result<PaginationResponse<AgentResponse>>> HandleAsync(
        AgentByRequest request,
        CancellationToken ct)
    {
        logger.LogInformation("Getting Agents");

        var query = agentRepo.Query;

        if (!string.IsNullOrWhiteSpace(request.Email))
            query = query.Where(a => a.Email == request.Email);

        var total = request.SkipTotal ? -1 : await agentRepo.CountAsync(query, ct);

        query = query
            .Skip(request.Skip)
            .Take(request.Limit);

        var agents = await agentRepo.GetListAsync(
            query: query,
            selector: a => new AgentResponse(a.Id, a.Name, a.Email, ""),
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
