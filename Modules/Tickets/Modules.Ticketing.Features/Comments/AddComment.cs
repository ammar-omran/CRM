using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Domain.Entities;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Comments;

public sealed record AddCommentRequest(
		string Content,
		bool IsAdmin = false,
		int? CreatedBy = null,
		string? CreatedByName = null
);

public sealed record AddCommentResponse(int Id, string Message, string MessageAr);

public interface IAddCommentHandler : IHandler
{
	Task<Result<AddCommentResponse>> HandleAsync(int ticketId, AddCommentRequest request, CurrentUser currentUser, CancellationToken cancellationToken = default);
}

internal sealed class AddCommentHandler(
		TicketingDbContext db,
		ILogger<AddCommentHandler> logger) : IAddCommentHandler
{
	public async Task<Result<AddCommentResponse>> HandleAsync(
		int ticketId,
		AddCommentRequest request,
		CurrentUser currentUser,
		CancellationToken ct = default)
	{
		var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId, ct);
		if (ticket is null)
			return Error.NotFound("Ticket.NotFound", $"No ticket found with Id {ticketId}");

		// Determine actor — ported from TicketBusiness.AddCommentAsync
		int? createdById;
		string? createdByName;
		if (request.IsAdmin)
		{
			createdById = request.CreatedBy ?? (int.TryParse(currentUser.UserId, out var uid) && uid != 0 ? uid : 1);
			createdByName = !string.IsNullOrWhiteSpace(request.CreatedByName) ? request.CreatedByName
					: (!string.IsNullOrWhiteSpace(currentUser.Name) ? currentUser.Name : "superadmin");
		}
		else
		{
			createdById = request.CreatedBy ?? (int.TryParse(currentUser.UserId, out var uid2) && uid2 != 0 ? uid2 : null);
			createdByName = !string.IsNullOrWhiteSpace(request.CreatedByName) ? request.CreatedByName
					: (!string.IsNullOrWhiteSpace(currentUser.Name) ? currentUser.Name : null);
			if (string.IsNullOrWhiteSpace(createdByName))
				createdByName = "Customer";
			if (createdById == null || createdById == 0)
				createdById = 1; // fallback to avoid validation failure when auth not present
		}

		// Create comment via domain factory (validates Content length, TicketId, Commenter)
		var createResult = TicketComment.Create(ticketId, request.Content, createdById.Value, createdByName, request.IsAdmin);
		if (createResult.IsError)
			return createResult.Errors;

		var comment = createResult.Value!;
		db.TicketComments.Add(comment);

		// If admin comment, treat as staff action: ensure ticket moves Open->InProgress and record history
		if (request.IsAdmin)
		{
			var operatorId = createdById.Value;
			var operatorName = createdByName ?? "superadmin";

			// Try to resolve Operator for history; fallback to raw ids
			var op = await db.Operators.FirstOrDefaultAsync(o => o.RefId == currentUser.UserId, ct);
			if (op is null && !string.IsNullOrWhiteSpace(currentUser.UserId))
			{
				op = new Operator
				{
					RefId = currentUser.UserId!,
					Name = currentUser.Name ?? operatorName,
					Email = currentUser.Email ?? string.Empty
				};
				db.Operators.Add(op);
				await db.SaveChangesAsync(ct);
			}

			if (ticket.Status == TicketStatusEnum.Open)
			{
				var actor = op ?? new Operator { Id = operatorId, RefId = operatorId.ToString(), Name = operatorName, Email = string.Empty };
				// Use domain method to ensure history is recorded
				var statusError = ticket.ChangeStatus(TicketStatusEnum.InProgress, actor);
				if (statusError is not null)
					logger.LogWarning("Failed to transition ticket {TicketId} status on admin comment: {Error}", ticketId, statusError.Value.Description);
			}
		}

		try
		{
			await db.SaveChangesAsync(ct);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to save comment for ticket {TicketId}", ticketId);
			return Error.Failure("Comment.DbError", "Failed to save comment.");
		}

		// Email notification (best-effort, ignore failure as in source)
		// Original sends to ticket.CustomerEmail; target Ticket has no CustomerEmail, so skip.

		return new AddCommentResponse(comment.Id, "Comment sent", "تم إرسال الرد");
	}
}

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
	public AddCommentRequestValidator()
	{
		RuleFor(x => x.Content)
				.NotEmpty().WithMessage("Content is required")
				.MaximumLength(1000).WithMessage("Content must be 1000 characters or fewer");
	}
}
