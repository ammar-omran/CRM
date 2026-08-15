using Modules.Ticketing.Domain.Entities;

namespace Modules.Ticketing.Domain.Auditing;

public static class TicketAuditHelper
{
	public const string CreatedField = "Created";
	public const string OperatorsField = "Operators";

	private static readonly Dictionary<string, (string Display, string Description)> Templates = new()
	{
		{ CreatedField, ("Created", "Ticket Created By {NewValue}") },
		{ OperatorsField, ("Operators", "Ticket Operators changed from '{OldValue}' to '{NewValue}'.") },
		{ nameof(Ticket.Status), ("Status Changed", "Status Changed from {OldValue} to '{NewValue}'") },
		{ nameof(Ticket.CategoryId), ("Category", "Ticket Category changed from '{OldValue}' to '{NewValue}'.") },
		{ nameof(Ticket.TypeId), ("Type", "Ticket Type changed from '{OldValue}' to '{NewValue}'.") },
		{ nameof(Ticket.SeverityId), ("Severity", "Severity Update from {OldValue} to '{NewValue}'") },
		{ nameof(Ticket.Title), ("Title", "Ticket Title changed from '{OldValue}' to '{NewValue}'.") },
		{ nameof(Ticket.TitleId), ("Title", "Ticket Title changed from '{OldValue}' to '{NewValue}'.") },
		{ nameof(Ticket.Description), ("Description", "Ticket Description changed from '{OldValue}' to '{NewValue}'.") },
	};

	public static string GetDisplayedName(string fieldName)
	{
		return Templates.TryGetValue(fieldName, out var template) ? template.Display : fieldName;
	}

	public static string GetDescription(string fieldName, string newValue, string? oldValue)
	{
		if (Templates.TryGetValue(fieldName, out var template))
		{
			return template.Description
				.Replace("{OldValue}", oldValue ?? string.Empty)
				.Replace("{NewValue}", newValue);
		}

		return $"{fieldName}: {newValue}";
	}
}
