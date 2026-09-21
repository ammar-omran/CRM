using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Comments;

public sealed record GetTicketCommentsRequest(int TicketId);

public sealed record TicketCommentResponse(
		int CommentId,
		int CreatedById,
		string CreatedByName,
		string Description,
		DateTime CreatedDate,
		bool HaveAttachments,
		bool IsAdmin
);

public interface IGetTicketCommentsHandler : IHandler
{
	Task<Result<IReadOnlyList<TicketCommentResponse>>> HandleAsync(int ticketId, CancellationToken cancellationToken = default);
	Task<Result<IReadOnlyList<TicketCommentResponse>>> HandleAsync(GetTicketCommentsRequest request, CancellationToken cancellationToken = default);
}

internal sealed class GetTicketCommentsHandler(
		TicketingDbContext db,
		ILogger<GetTicketCommentsHandler> logger) : IGetTicketCommentsHandler
{
	public Task<Result<IReadOnlyList<TicketCommentResponse>>> HandleAsync(int ticketId, CancellationToken ct = default)
			=> HandleAsync(new GetTicketCommentsRequest(ticketId), ct);

	public async Task<Result<IReadOnlyList<TicketCommentResponse>>> HandleAsync(
			GetTicketCommentsRequest request,
			CancellationToken ct = default)
	{
		var ticketId = request.TicketId;

		// Ensure ticket exists (optional, but helpful for 404 semantics)
		var ticketExists = await db.Tickets.AsNoTracking().AnyAsync(t => t.Id == ticketId, ct);
		if (!ticketExists)
			return Error.NotFound("Ticket.NotFound", $"No ticket found with Id {ticketId}");

		try
		{
			var comments = await db.TicketComments
					.AsNoTracking()
					.Where(c => c.TicketId == ticketId)
					.OrderByDescending(c => c.CreatedDate)
					.ToListAsync(ct);

			if (comments.Count == 0)
				return new List<TicketCommentResponse>();

			var commentIds = comments.Select(c => c.Id).ToList();

			// Check attachments per comment via generic Attachment table (ReferenceId == comment Id)
			var attachmentsByRef = await db.Attachments
					.AsNoTracking()
					.Where(a => a.ReferenceId != null && commentIds.Contains(a.ReferenceId.Value))
					.GroupBy(a => a.ReferenceId!.Value)
					.ToDictionaryAsync(g => g.Key, g => g.Any(), ct);

			var result = comments.Select(comment => new TicketCommentResponse(
					CommentId: comment.Id,
					CreatedById: comment.Commenter,
					CreatedByName: string.IsNullOrWhiteSpace(comment.CreatedByName) ? "N/A" : comment.CreatedByName!,
					Description: comment.Content,
					CreatedDate: comment.CreatedDate,
					HaveAttachments: attachmentsByRef.TryGetValue(comment.Id, out var has) && has,
					IsAdmin: comment.IsAdmin
			)).ToList();

			return result;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error retrieving ticket comments for ticket {TicketId}", ticketId);
			return Error.Failure("Comment.DbError", $"Error retrieving ticket comments: {ex.Message}");
		}
	}
}

public class GetTicketCommentsRequestValidator : AbstractValidator<GetTicketCommentsRequest>
{
	public GetTicketCommentsRequestValidator()
	{
		RuleFor(x => x.TicketId).GreaterThan(0).WithMessage("TicketId must be greater than 0");
	}
}
