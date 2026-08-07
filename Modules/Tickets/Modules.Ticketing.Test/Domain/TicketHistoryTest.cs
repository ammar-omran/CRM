using TicketManagement.Domain.OrganizationAggregate.
using Xunit;

namespace TicketManagement.Test.Domain;

public class TicketHistoryTests
{
    [Fact]
    public void CreateChanges_ShouldTrackOnlyModified_TrackableFields()
    {
        // Arrange
        var originalTicket = new Ticket
        {
            Id = 1,
            Status = TicketStatusEnum.Open,
            CreatedDate = new DateTime(2024, 1, 1),
            SeverityId = 2,
            TypeId = 3,
            CategoryId = 5,
            Title = "Failed Recharge",
            category = new Category { Name = "Original Category" },
            Type = new TicketType { Name = "Original Type" },
            Severity = new Severity { Name = "Original Severity" },
            TicketTitle = new TicketTitle { Name = "Original Title" }
        };

        var updatedTicket = new Ticket
        {
            Id = 1,
            Status = TicketStatusEnum.InProgress, // changed
            CreatedDate = new DateTime(2024, 1, 1), // unchanged
            SeverityId = 9, // changed
            TypeId = 3, // unchanged
            CategoryId = 6, // changed
            Title = "Updated Title", // changed
            category = new Category { Name = "New Category" },
            Type = new TicketType { Name = "Original Type" },
            Severity = new Severity { Name = "New Severity" },
            TicketTitle = new TicketTitle { Name = "Original Title" }
        };

        int userId = 10;
        string userName = "TestUser";

        // Act
        var histories = TicketHistory.CreateChanges(originalTicket, updatedTicket, userId, userName).ToList();

        // Assert
        Assert.NotEmpty(histories);
        Assert.Equal(4, histories.Count); // Only changed and trackable fields

        var fieldNames = histories.Select(h => h.FieldName).ToHashSet();

        Assert.Contains(nameof(Ticket.Status), fieldNames);
        Assert.Contains(nameof(Ticket.CategoryId), fieldNames);

        // Example: Assert a specific field history
        var statusHistory = histories.First(h => h.FieldName == nameof(Ticket.Status));
        Assert.Equal("Open", statusHistory.OldValue);
        Assert.Equal("InProgress", statusHistory.NewValue);
        Assert.Equal("Status Changed", statusHistory.Title);
        Assert.Equal("Status Changed from Open to 'InProgress'", statusHistory.Description);
        Assert.Equal(userId, statusHistory.CreatedBy);
        Assert.Equal(userName, statusHistory.CreatedByName);
    }
}
