using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Database;
using Modules.Organizations.Domain.Entities;
using Modules.Organizations.Features.Agent.Shared.Errors;
using Modules.Organizations.Features.Agent.Shared.Requests;
using Modules.Organizations.Features.Agent.Shared.Responses;

namespace Modules.Organizations.Features.Agent.CreateAgent;

internal interface ICreateAgentHandler : IHandler
{
    Task<Result<AgentResponse>> HandleAsync(AddAgentRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateAgentHandler(
    IRepository<Domain.Entities.Agent> agentRepo,
    IReadRepository<Domain.Entities.Agent> agentReadRepo,
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

        var agent = new Domain.Entities.Agent
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
