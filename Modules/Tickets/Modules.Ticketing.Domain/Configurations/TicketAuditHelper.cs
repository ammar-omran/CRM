using TicketManagement.Domain.Entities;

namespace TicketManagement.Domain.Configurations;

public static class TicketAuditHelper
{

    /// <summary>
    /// A Dictionary of string field names in the <see cref="Ticket"/> entity that are considered trackable for audit logging.
    /// Only changes to these fields will be recorded in ticket history.
    /// </summary>
    public static readonly Dictionary<string, (string, string)> TrackableFields = new()
    {
        // { fieldKey/propertyName, (FieldDisplayName, Description) }
        { nameof(Ticket.Status), ("Status Changed", "Status Changed from {OldValue} to '{NewValue}'") },
        { nameof(Ticket.CreatedDate), ("Created", "Ticket Created By {NewValue}") },
        { nameof(Ticket.SeverityId), ("Severity", "Severity Update from {OldValue} to '{NewValue}'") },
        { nameof(Ticket.UpdatedBy), ("Assigned", "Ticket Assigned to '{NewValue}'") },
        { nameof(Ticket.TypeId), ("Type", "Ticket Type changed from '{OldValue}' to '{NewValue}'.") },
        { nameof(Ticket.CategoryId), ("Category", "Ticket Category changed from '{OldValue}' to '{NewValue}'.") },
    };
    public static string GetDisplayedName(string propName)
    {
        return TrackableFields.TryGetValue(propName, out (string, string) value) ? value.Item1 : propName;
    }

    public static string GetDescription(string fieldName, string newValue, string? oldValue)
    {
        return TrackableFields.TryGetValue(fieldName, out (string, string) value)
            ? string.IsNullOrEmpty(value.Item2)
                ? $"{value.Item1}: {newValue}" // No specific description provided
                : value.Item2.Replace("{OldValue}", oldValue ?? "").Replace("{NewValue}", newValue)
            : $"{fieldName}: {newValue}";
    }
    public static string FormatValue(string propName, object? value, Ticket ticket)
    {
        if (value is null) return string.Empty;

        return propName switch
        {
            nameof(Ticket.CategoryId) => ticket.category?.Name ?? value.ToString(),
            nameof(Ticket.TypeId) => ticket.Type?.Name ?? value.ToString(),
            nameof(Ticket.SeverityId) => ticket.Severity?.Name ?? value.ToString(),
            nameof(Ticket.UpdatedBy) => ticket.UpdatedByName ?? value.ToString(), // Assigned To
            nameof(Ticket.Status) => value.ToString(), // assuming Status is an int or enum
            _ => value.ToString() ?? string.Empty
        } ?? string.Empty;
    }
}
