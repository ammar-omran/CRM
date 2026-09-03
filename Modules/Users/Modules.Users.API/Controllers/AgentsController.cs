using CRM.SharedKernel.Application.API.Extensions;
using CRM.SharedKernel.Application.API.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Users.Domain.Policies;
using Modules.Users.Features.Agents.CreateAgent;
using Modules.Users.Features.Agents.GetAgents;
using Modules.Users.Features.Agents.Shared.Requests;
using Modules.Users.Features.Agents.Shared.Responses;

namespace Modules.Users.API.Controllers;

/// <summary>
/// Agent workforce endpoints.
/// </summary>
[ApiController]
[Route("api/agents")]
[Produces("application/json")]
public sealed class AgentsController : ControllerBase
{
    /// <summary>
    /// Create a new agent from an existing user.
    /// </summary>
    /// <remarks>Links an existing identity user as an agent via UserId. Requires agent create permission.</remarks>
    [HttpPost]
    [Authorize(Policy = UserPolicyConstants.AgentCreatePolicy)]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] AddAgentRequest request,
        [FromServices] IValidator<AddAgentRequest> validator,
        [FromServices] ICreateAgentHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }

    /// <summary>
    /// Get a paginated list of agents.
    /// </summary>
    /// <remarks>Requires agent read permission. Supports filtering by UserId and pagination.</remarks>
    [HttpGet]
    [Authorize(Policy = UserPolicyConstants.AgentReadPolicy)]
    [ProducesResponseType(typeof(PaginationResponse<AgentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Get(
        [FromQuery] AgentByRequest request,
        [FromServices] IValidator<AgentByRequest> validator,
        [FromServices] IGetAgentsHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }
}
