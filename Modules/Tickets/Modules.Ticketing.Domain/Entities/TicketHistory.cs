using Modules.Ticketing.Domain.Auditing;

namespace Modules.Ticketing.Domain.Entities;

public class TicketHistory
{
	private TicketHistory() { }

	public int Id { get; private set; }
	public int TicketId { get; private set; }
	public int OperatorId { get; private set; }
	public string OperatorName { get; private set; } = string.Empty;
	public string FieldName { get; private set; } = string.Empty;
	public string? OldValue { get; private set; }
	public string NewValue { get; private set; } = string.Empty;
	public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;

	public Ticket Ticket { get; private set; } = default!;

	public string Title => TicketAuditHelper.GetDisplayedName(FieldName);
	public string Description => TicketAuditHelper.GetDescription(FieldName, NewValue, OldValue);

	public static TicketHistory ForCreated(int ticketId, Operator createdBy) => new()
	{
		TicketId = ticketId,
		OperatorId = createdBy.Id,
		OperatorName = createdBy.Name,
		FieldName = TicketAuditHelper.CreatedField,
		NewValue = createdBy.Name,
	};

	public static TicketHistory ForChange(string fieldName, string? oldValue, string newValue, Operator actedBy) => new()
	{
		OperatorId = actedBy.Id,
		OperatorName = actedBy.Name,
		FieldName = fieldName,
		OldValue = oldValue,
		NewValue = newValue,
	};
}
