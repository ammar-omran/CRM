using CRM.SharedKernel.Application.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Ticketing.Features.Tickets;

namespace Modules.Ticketing.API.Controllers;

/// <summary>
/// Reference data for the ticket create form and list filters.
/// Authenticated-only (no claim policy): non-sensitive lookup data needed by
/// every ticket role, including pure creators without view permissions.
/// </summary>
[ApiController]
[Route("api/tickets/lookups")]
public sealed class TicketLookupsController : ControllerBase
{
	[HttpGet("severities")]
	[Authorize]
	public async Task<IActionResult> GetSeverities(
			[FromServices] ITicketLookupsHandler handler,
			CancellationToken cancellationToken)
	{
		var result = await handler.GetSeveritiesAsync(cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	[HttpGet("categories")]
	[Authorize]
	public async Task<IActionResult> GetCategories(
			[FromServices] ITicketLookupsHandler handler,
			CancellationToken cancellationToken)
	{
		var result = await handler.GetCategoriesAsync(cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	[HttpGet("types")]
	[Authorize]
	public async Task<IActionResult> GetTypes(
			[FromServices] ITicketLookupsHandler handler,
			CancellationToken cancellationToken)
	{
		var result = await handler.GetTypesAsync(cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	[HttpGet("services")]
	[Authorize]
	public async Task<IActionResult> GetServices(
			[FromServices] ITicketLookupsHandler handler,
			CancellationToken cancellationToken)
	{
		var result = await handler.GetServicesAsync(cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	[HttpGet("statuses")]
	[Authorize]
	public async Task<IActionResult> GetStatuses(
			[FromServices] ITicketLookupsHandler handler,
			CancellationToken cancellationToken)
	{
		var result = await handler.GetStatusesAsync(cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	/// <summary>
	/// Title options, optionally narrowed by category.
	/// Omit <paramref name="categoryId"/> for the full list.
	/// </summary>
	[HttpGet("titles")]
	[Authorize]
	public async Task<IActionResult> GetTitles(
			[FromQuery] int? categoryId,
			[FromServices] ITicketLookupsHandler handler,
			CancellationToken cancellationToken)
	{
		var result = await handler.GetTitlesAsync(categoryId, cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}
}
