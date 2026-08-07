using System.Net.Sockets;
using TicketManagement.Domain.Configurations;

namespace TicketManagement.Domain.Entities;

public class TicketHistory
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string FieldName { get; set; } = nameof(Ticket.Status);
    public string? OldValue { get; set; }
    public string NewValue { get; set; } = string.Empty;
    public string Title => TicketAuditHelper.GetDisplayedName(FieldName);
    public string Description => TicketAuditHelper.GetDescription(FieldName, NewValue, OldValue);

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public Ticket Ticket { get; set; } = default!;

    public TicketHistory() { }

    /// <summary>
    /// Compares the old and updated ticket entities and generates a list of TicketHistory entries
    /// for each changed field that exists in <c><see cref="TicketAuditHelper.TrackableFields"/></c>.
    /// </summary>
    /// <param name="oldTicket">The original state of the ticket before the update.</param>
    /// <param name="updatedTicket">The new state of the ticket after the update.</param>
    /// <param name="createdBy">The ID of the user who made the change.</param>
    /// <param name="createdByName">The display name of the user who made the change.</param>
    /// <returns>
    /// A list of <see cref="TicketHistory"/> records representing each change detected in the trackable fields.
    /// </returns>
    public static IEnumerable<TicketHistory> CreateChanges(
        Ticket original,
        Ticket? updated,
        int createdBy,
        string createdByName)
    {
        var changes = new List<TicketHistory>();
        var trackable = TicketAuditHelper.TrackableFields;

        if (updated is null)
            return [new TicketHistory
            {
                TicketId = original.Id,
                FieldName = nameof(Ticket.CreatedDate),
                NewValue = createdByName,
                CreatedBy = createdBy,
                CreatedByName = createdByName
            }];

        var type = typeof(Ticket);
        foreach (var prop in type.GetProperties())
        {
            if (!trackable.ContainsKey(prop.Name)) continue;

            var oldValRaw = prop.GetValue(original);
            var newValRaw = prop.GetValue(updated);

            // Skip if no change
            if (Equals(oldValRaw, newValRaw)) continue;

            string oldVal = TicketAuditHelper.FormatValue(prop.Name, oldValRaw, original);
            string newVal = TicketAuditHelper.FormatValue(prop.Name, newValRaw, updated);

            changes.Add(new TicketHistory
            {
                TicketId = updated.Id,
                FieldName = prop.Name,
                OldValue = oldVal,
                NewValue = newVal,
                CreatedDate = DateTime.Now,
                CreatedBy = createdBy,
                CreatedByName = createdByName,
            });
        }

        return changes;
    }
}
