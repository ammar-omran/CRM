using CRM.SharedKernel.Application.API.Extensions;
using CRM.SharedKernel.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Ticketing.Domain.Policies;
using Modules.Ticketing.Features.Tickets;
using Modules.Ticketing.Features.Tickets.Shared;

namespace Modules.Ticketing.API.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController : ControllerBase
{
	private readonly ICurrentUserService _currentUserService;

	public TicketsController(ICurrentUserService currentUserService)
	{
		_currentUserService = currentUserService;
	}

	/// <summary>
	/// Unscoped paginated list with filtering. Policy-gated (supervisor/admin).
	/// Isolation-free by design — use /mine or /group for scoped views.
	/// </summary>
	[HttpGet("list")]
	[Authorize(Policy = TicketPolicyConstants.ViewAllPolicy)]
	public async Task<IActionResult> GetList(
			[FromQuery] TicketFilterRequest request,
			[FromServices] IGetTicketsListHandler handler,
			CancellationToken cancellationToken)
	{
		var result = await handler.HandleAsync(request, cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	/// <summary>
	/// Tickets assigned to the caller (TicketOperators → Operator.RefId == UserId).
	/// </summary>
	[HttpGet("mine")]
	[Authorize(Policy = TicketPolicyConstants.ViewMinePolicy)]
	public async Task<IActionResult> GetMine(
			[FromQuery] TicketFilterRequest request,
			[FromServices] IGetMyTicketsHandler handler,
			CancellationToken cancellationToken)
	{
		var result = await handler.HandleAsync(request, _currentUserService.CurrentUser, cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	/// <summary>
	/// Tickets sharing the caller's group (ticket.GroupId == tokenPayload[GroupKey]).
	/// </summary>
	[HttpGet("group")]
	[Authorize(Policy = TicketPolicyConstants.ViewGroupPolicy)]
	public async Task<IActionResult> GetGroup(
			[FromQuery] TicketFilterRequest request,
			[FromServices] IGetGroupTicketsHandler handler,
			CancellationToken cancellationToken)
	{
		var result = await handler.HandleAsync(request, _currentUserService.CurrentUser, cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	// :int constraints keep single-segment routes from colliding with
	// multi-segment routes such as lookups/* under the same prefix.
	[HttpGet("{ticketId:int}")]
	[Authorize(Policy = TicketPolicyConstants.ViewAnyPolicy)]
	public async Task<IActionResult> GetTicketById(
			int ticketId,
			[FromServices] IGetTicketByIdHandler getTicketByIdHandler,
			CancellationToken cancellationToken)
	{
		var result = await getTicketByIdHandler.HandleAsync(ticketId, _currentUserService.CurrentUser, cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	/// <summary>
	/// Raw audit trail — strong DTOs for frontend formatting/i18n (no server-side text).
	/// </summary>
	[HttpGet("{ticketId:int}/history")]
	[Authorize(Policy = TicketPolicyConstants.ViewAnyPolicy)]
	public async Task<IActionResult> GetTicketHistory(
			int ticketId,
			[FromServices] IGetTicketHistoryHandler handler,
			CancellationToken cancellationToken)
	{
		var result = await handler.HandleAsync(ticketId, _currentUserService.CurrentUser, cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	[HttpPost]
	[Authorize(Policy = TicketPolicyConstants.CreatePolicy)]
	public async Task<IActionResult> Create(
			[FromBody] CreateTicketRequest request,
			[FromServices] ICreateTicketHandler createTicketHandler,
			CancellationToken cancellationToken)
	{
		var result = await createTicketHandler.HandleAsync(request, _currentUserService.CurrentUser, cancellationToken);
		return result.IsError
			? result.Errors.ToMVCProblem()
			: Created($"api/tickets/{result.Value?.Id}", result.Value);
	}

	[HttpPost("{ticketId:int}/assign")]
	[Authorize(Policy = TicketPolicyConstants.AssignPolicy)]
	public async Task<IActionResult> Assign(
			int ticketId,
			[FromBody] AssignTicketRequest request,
			[FromServices] IAssignTicketHandler handler,
			CancellationToken cancellationToken)
	{
		var result = await handler.HandleAsync(ticketId, request, _currentUserService.CurrentUser, cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}
}


