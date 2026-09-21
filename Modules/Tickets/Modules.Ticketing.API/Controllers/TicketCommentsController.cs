using CRM.SharedKernel.Application.API.Extensions;
using CRM.SharedKernel.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Ticketing.Domain.Policies;
using Modules.Ticketing.Features.Comments;

namespace Modules.Ticketing.API.Controllers;

/// <summary>
/// Ticket comments (replies) — ported from TicketManagement.API.Controllers.TicketControllers
/// (AddComment / GetTicketComments) with DbContext + FluentValidation + handler pattern.
/// </summary>
[ApiController]
[Route("api/tickets")]
public sealed class TicketCommentsController : ControllerBase
{
	private readonly ICurrentUserService _currentUserService;

	public TicketCommentsController(ICurrentUserService currentUserService)
	{
		_currentUserService = currentUserService;
	}

	private static Dictionary<string, string[]> ToDictionary(FluentValidation.Results.ValidationResult validation)
			=> validation.Errors
					.GroupBy(e => e.PropertyName)
					.ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

	/// <summary>
	/// Get comments for a ticket, ordered by created date desc, with HaveAttachments flag.
	/// Legacy route: GET api/tickets/GetTicketComments/{ticketId}
	/// REST route:   GET api/tickets/{ticketId}/comments
	/// </summary>
	[HttpGet("{ticketId:int}/comments")]
	[Authorize(Policy = CommentPolicyConstants.ViewPolicy)]
	public async Task<IActionResult> GetTicketComments(
			int ticketId,
			[FromServices] IValidator<GetTicketCommentsRequest> validator,
			[FromServices] IGetTicketCommentsHandler handler,
			CancellationToken ct)
	{
		var request = new GetTicketCommentsRequest(ticketId);
		var validation = await validator.ValidateAsync(request, ct);
		if (!validation.IsValid)
			return BadRequest(ToDictionary(validation));

		var result = await handler.HandleAsync(request, ct);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}

	/// <summary>
	/// Add a comment/reply to a ticket.
	/// Legacy route: POST api/tickets/{ticketId}/addComment
	/// REST route:   POST api/tickets/{ticketId}/comments
	/// Body: { content, isAdmin?, createdBy?, createdByName? } — createdBy fields are optional, server prefers CurrentUser.
	/// </summary>
	[HttpPost("{ticketId:int}/comments")]
	[Authorize(Policy = CommentPolicyConstants.AddPolicy)]
	public async Task<IActionResult> AddComment(
			int ticketId,
			[FromBody] AddCommentRequest request,
			[FromServices] IValidator<AddCommentRequest> validator,
			[FromServices] IAddCommentHandler handler,
			CancellationToken ct)
	{
		var validation = await validator.ValidateAsync(request, ct);
		if (!validation.IsValid)
			return BadRequest(ToDictionary(validation));

		var result = await handler.HandleAsync(ticketId, request, _currentUserService.CurrentUser, ct);
		return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
	}
}
