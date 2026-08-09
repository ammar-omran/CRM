using CRM.SharedKernel.Application.API.Extensions;
using CRM.SharedKernel.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Ticketing.Domain.Policies;
using Modules.Ticketing.Features.Ticket.CreateTicket;
using Modules.Ticketing.Features.Ticket.DeleteTicket;
using Modules.Ticketing.Features.Ticket.GetTicketById;
using Modules.Ticketing.Features.Ticket.GetTickets;
using Modules.Ticketing.Features.Ticket.UpdateTicket;

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

	[HttpGet]
	[Authorize(Policy = TicketPolicyConstants.ViewPolicy)]
	public async Task<IActionResult> GetTickets(
			[FromServices] IGetTicketsHandler getTicketsHandler,
			CancellationToken cancellationToken)
	{
		var result = await getTicketsHandler.HandleAsync(cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	[HttpGet("{ticketId}")]
	[Authorize(Policy = TicketPolicyConstants.ViewPolicy)]
	public async Task<IActionResult> GetTicketById(
			int ticketId,
			[FromServices] IGetTicketByIdHandler getTicketByIdHandler,
			CancellationToken cancellationToken)
	{
		var result = await getTicketByIdHandler.HandleAsync(ticketId, cancellationToken);
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

	[HttpPut("{ticketId}")]
	[Authorize(Policy = TicketPolicyConstants.UpdatePolicy)]
	public async Task<IActionResult> Update(
			int ticketId,
			[FromServices] IUpdateTicketHandler updateTicketHandler,
			[FromBody] UpdateTicketRequest request, CancellationToken cancellationToken)
	{
		if (ticketId != request.Id)
		{
			return BadRequest(new { Error = "Ticket ID mismatch" });
		}

		var result = await updateTicketHandler.HandleAsync(request, _currentUserService.CurrentUser, cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	[HttpDelete("{ticketId}")]
	[Authorize(Policy = TicketPolicyConstants.DeletePolicy)]
	public async Task<IActionResult> Delete(
			int ticketId,
			[FromServices] IDeleteTicketHandler deleteTicketHandler,
			CancellationToken cancellationToken)
	{
		var result = await deleteTicketHandler.HandleAsync(ticketId, cancellationToken);
		return result.IsError ? result.Errors.ToMVCProblem() : NoContent();
	}
}
