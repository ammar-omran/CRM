using CRM.SharedKernel.Domain;
using CRM.SharedKernel.Domain.Results;
using Modules.Ticketing.Domain.Configurations;
using Modules.Ticketing.Domain.Errors;

namespace Modules.Ticketing.Domain.Entities;

public class Ticket : IAuditableEntity
{
	private const int TitleMaxLength = 100;
	private const int DescriptionMaxLength = 1000;

	private static readonly Dictionary<TicketStatusEnum, TicketStatusEnum[]> AllowedStatusTransitions = new()
	{
		[TicketStatusEnum.Open] = [TicketStatusEnum.InProgress, TicketStatusEnum.Resolved, TicketStatusEnum.Closed],
		[TicketStatusEnum.InProgress] = [TicketStatusEnum.Resolved, TicketStatusEnum.Closed],
		[TicketStatusEnum.Resolved] = [TicketStatusEnum.Closed],
		[TicketStatusEnum.Closed] = [],
	};

	private Ticket() { }

	public int Id { get; private set; }
	public int CategoryId { get; private set; }
	public int? TitleId { get; private set; }
	public int TypeId { get; private set; }
	public int? SeverityId { get; private set; }
	public string? GroupId { get; private set; }
	public string? OtherTitle { get; private set; }
	public string? Title => TicketTitle?.Name ?? OtherTitle;
	public string Description { get; private set; } = default!;
	public TicketStatusEnum Status { get; private set; } = TicketStatusEnum.Open;
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }

	public List<TicketHistory> TicketHistories { get; private set; } = [];
	public List<TicketComment> TicketComments { get; private set; } = [];
	public List<TicketOperator> TicketOperators { get; private set; } = [];

	public Category Category { get; private set; } = default!;
	public Severity Severity { get; private set; } = default!;
	public TicketTitle? TicketTitle { get; private set; }
	public TicketType Type { get; private set; } = default!;

	public static Result<Ticket> Create(
		string? otherTitle,
		string description,
		Category category,
		TicketType type,
		Operator createdBy,
		string? creatorRole = null,
		Severity? severity = null,
		TicketTitle? ticketTitle = null,
		string? groupId = null)
	{
		if (category is null)
			return TicketErrors.CategoryRequired;

		if (type is null)
			return TicketErrors.TypeRequired;

		if (createdBy is null)
			return TicketErrors.OperatorRequired;

		if (ticketTitle is not null && ticketTitle.CategoryId != category.Id)
			return TicketErrors.InvalidTitleCategory;

		var ticket = new Ticket
		{
			OtherTitle = ticketTitle is null ? otherTitle : null,
			Description = description,
			CategoryId = category.Id,
			Category = category,
			TypeId = type.Id,
			Type = type,
			SeverityId = severity?.Id,
			Severity = severity!,
			TitleId = ticketTitle?.Id,
			TicketTitle = ticketTitle,
			GroupId = groupId,
		};

		var errors = ticket.Validate();
		if (errors.Length > 0)
			return errors;

		ticket.CreatedAt = DateTime.UtcNow;
		ticket.TicketHistories.Add(TicketHistory.ForCreated(ticket.Id, createdBy));

		if (!string.IsNullOrWhiteSpace(creatorRole))
			ticket.TicketOperators.Add(new TicketOperator
			{
				TicketId = ticket.Id,
				OperatorId = createdBy.Id,
				Role = creatorRole,
				Operator = createdBy
			});

		return ticket;
	}

	public Error[] Validate()
	{
		var errors = new List<Error>();

		if (TitleId is null && string.IsNullOrWhiteSpace(OtherTitle))
			errors.Add(TicketErrors.TitleRequired);

		if (!string.IsNullOrWhiteSpace(OtherTitle) && OtherTitle.Length > TitleMaxLength)
			errors.Add(Error.Validation("Ticket.InvalidTitle", $"Title must be {TitleMaxLength} characters or fewer."));

		if (string.IsNullOrWhiteSpace(Description) || Description.Length > DescriptionMaxLength)
			errors.Add(Error.Validation("Ticket.InvalidDescription", $"Description is required and must be {DescriptionMaxLength} characters or fewer."));

		if (CategoryId <= 0)
			errors.Add(TicketErrors.CategoryRequired);

		if (TypeId <= 0)
			errors.Add(TicketErrors.TypeRequired);

		return errors.ToArray();
	}

	public Error? SetTitle(TicketTitle ticketTitle, Operator actedBy)
	{
		if (actedBy is null)
			return TicketErrors.OperatorRequired;

		if (ticketTitle is null)
			return TicketErrors.TitleRequired;

		if (ticketTitle.CategoryId != CategoryId)
			return TicketErrors.InvalidTitleCategory;

		var oldValue = Title;
		TitleId = ticketTitle.Id;
		TicketTitle = ticketTitle;
		OtherTitle = null;
		RecordChange(nameof(Title), oldValue, ticketTitle.Name, actedBy);
		return null;
	}

	public Error? SetOtherTitle(string otherTitle, Operator actedBy)
	{
		if (actedBy is null)
			return TicketErrors.OperatorRequired;

		if (string.IsNullOrWhiteSpace(otherTitle) || otherTitle.Length > TitleMaxLength)
			return Error.Validation("Ticket.InvalidTitle", $"Title is required and must be {TitleMaxLength} characters or fewer.");

		var oldValue = Title;
		OtherTitle = otherTitle;
		TitleId = null;
		TicketTitle = null;
		RecordChange(nameof(Title), oldValue, otherTitle, actedBy);
		return null;
	}

	public Error? SetDescription(string description, Operator actedBy)
	{
		if (actedBy is null)
			return TicketErrors.OperatorRequired;

		if (string.IsNullOrWhiteSpace(description) || description.Length > DescriptionMaxLength)
			return Error.Validation("Ticket.InvalidDescription", $"Description is required and must be {DescriptionMaxLength} characters or fewer.");

		var oldValue = Description;
		Description = description;
		RecordChange(nameof(Description), oldValue, description, actedBy);
		return null;
	}

	public Error? SetCategory(Category category, Operator actedBy)
	{
		if (actedBy is null)
			return TicketErrors.OperatorRequired;

		if (category is null)
			return TicketErrors.CategoryRequired;

		if (TicketTitle is not null && TicketTitle.CategoryId != category.Id)
			return TicketErrors.InvalidTitleCategory;

		var oldValue = CategoryId.ToString();
		CategoryId = category.Id;
		Category = category;
		RecordChange(nameof(CategoryId), oldValue, category.Name, actedBy);
		return null;
	}

	public Error? SetType(TicketType type, Operator actedBy)
	{
		if (actedBy is null)
			return TicketErrors.OperatorRequired;

		if (type is null)
			return TicketErrors.TypeRequired;

		var oldValue = TypeId.ToString();
		TypeId = type.Id;
		Type = type;
		RecordChange(nameof(TypeId), oldValue, type.Name, actedBy);
		return null;
	}

	public Error? SetSeverity(Severity? severity, Operator actedBy)
	{
		if (actedBy is null)
			return TicketErrors.OperatorRequired;

		var oldValue = SeverityId?.ToString();
		SeverityId = severity?.Id;
		Severity = severity!;
		RecordChange(nameof(SeverityId), oldValue, severity?.Name ?? string.Empty, actedBy);
		return null;
	}

	public Error? ChangeStatus(TicketStatusEnum newStatus, Operator actedBy)
	{
		if (actedBy is null)
			return TicketErrors.OperatorRequired;

		if (newStatus == Status)
			return null;

		if (!AllowedStatusTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
			return TicketErrors.InvalidStatusTransition;

		var oldStatus = Status;
		Status = newStatus;
		RecordChange(nameof(Status), oldStatus.ToString(), newStatus.ToString(), actedBy);
		return null;
	}

	public Error? AssignOperator(Operator assignee, string role, Operator actedBy)
	{
		if (actedBy is null)
			return TicketErrors.OperatorRequired;

		if (assignee is null)
			return Error.Validation("Ticket.InvalidOperator", "Operator is required.");

		if (TicketOperators.Any(o => o.OperatorId == assignee.Id))
			return TicketErrors.AlreadyAssigned;

		var roleValue = string.IsNullOrWhiteSpace(role) ? "Operator" : role;
		TicketOperators.Add(new TicketOperator { TicketId = Id, OperatorId = assignee.Id, Role = roleValue, Operator = assignee });
		RecordChange(TicketAuditHelper.OperatorsField, null, $"{assignee.Id} ({roleValue})", actedBy);
		return null;
	}

	public Error? RemoveOperator(int operatorId, Operator actedBy)
	{
		if (actedBy is null)
			return TicketErrors.OperatorRequired;

		var ticketOperator = TicketOperators.FirstOrDefault(o => o.OperatorId == operatorId);
		if (ticketOperator is null)
			return Error.NotFound("Ticket.OperatorNotFound", $"Operator {operatorId} is not assigned to this ticket.");

		TicketOperators.Remove(ticketOperator);
		RecordChange(TicketAuditHelper.OperatorsField, operatorId.ToString(), string.Empty, actedBy);
		return null;
	}

	private void RecordChange(string fieldName, string? oldValue, string newValue, Operator actedBy)
	{
		UpdatedAt = DateTime.Now;
		TicketHistories.Add(TicketHistory.ForChange(fieldName, oldValue, newValue, actedBy));
	}
}
