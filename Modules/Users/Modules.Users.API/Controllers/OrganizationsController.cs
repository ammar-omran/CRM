using CRM.SharedKernel.Application.API.Extensions;
using CRM.SharedKernel.Application.API.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Users.Features.Organizations.CreateOrganization;
using Modules.Users.Features.Organizations.GetOrganizationCustomers;
using Modules.Users.Features.Organizations.GetOrganizations;
using Modules.Users.Features.Organizations.Shared.Requests;
using Modules.Users.Features.Organizations.Shared.Responses;

namespace Modules.Users.API.Controllers;

/// <summary>
/// Organization management endpoints.
/// </summary>
[ApiController]
[Route("api/organizations")]
[Produces("application/json")]
public sealed class OrganizationsController : ControllerBase
{
    /// <summary>
    /// Create a new organization with agents and customers.
    /// </summary>
    /// <remarks>Validates agent and role existence and assigns customers.</remarks>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] AddOrganizationRequest request,
        [FromServices] IValidator<AddOrganizationRequest> validator,
        [FromServices] ICreateOrganizationHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }

    /// <summary>
    /// Get a paginated list of organizations.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginationResponse<OrganizationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Get(
        [FromQuery] GetOrganizationRequest request,
        [FromServices] IValidator<GetOrganizationRequest> validator,
        [FromServices] IGetOrganizationsHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }

    /// <summary>
    /// Get customers belonging to organizations.
    /// </summary>
    [HttpGet("customers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginationResponse<OrganizationCustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] GetOrganizationCustomersRequest request,
        [FromServices] IValidator<GetOrganizationCustomersRequest> validator,
        [FromServices] IGetOrganizationCustomersHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }
}
