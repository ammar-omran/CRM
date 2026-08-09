using CRM.SharedKernel.Domain.Results;

namespace Modules.Ticketing.Domain.Entities;

public class TicketComment
{
	private const int ContentMaxLength = 1000;

	private TicketComment() { }

	public int Id { get; private set; }
	public string Content { get; private set; } = string.Empty;
	public int TicketId { get; private set; }
	public int Commenter { get; set; }
	public DateTime CreatedDate { get; private set; } = DateTime.Now;

	public Ticket Ticket { get; private set; } = default!;

	public static Result<TicketComment> Create(int ticketId, string content, int commenter)
	{
		var comment = new TicketComment
		{
			TicketId = ticketId,
			Content = content,
			Commenter = commenter,
		};

		var errors = comment.Validate();
		return errors.Length > 0 ? errors : comment;
	}

	public Error[] Validate()
	{
		var errors = new List<Error>();

		if (TicketId <= 0)
			errors.Add(Error.Validation("Comment.InvalidTicket", "A comment must belong to a ticket."));

		if (string.IsNullOrWhiteSpace(Content) || Content.Length > ContentMaxLength)
			errors.Add(Error.Validation("Comment.InvalidContent", $"Content is required and must be {ContentMaxLength} characters or fewer."));

		if (Commenter <= 0)
			errors.Add(Error.Validation("Comment.InvalidAutherId", "A comment must belong to a Commenter."));

		return errors.ToArray();
	}
}
