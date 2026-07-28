using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.JWT;
using TicketManagement.Application.Interfaces.Repositories;
using TicketManagement.Application.Services;
using TicketManagement.Domain.Entities;
using Xunit;

namespace TicketManagement.Test.Services.TicketService;

public class HistoryUnityTest
{
    [Fact]
    public async Task FetchTicketHistory_ShouldReturnMappedDTOs_WhenTicketExists()
    {
        // Arrange
        var ticketId = 123;

        var mockRepo = new Mock<ITicketRepository>();
        var mockEmailSender = new Mock<IEmailOperations>();
        var mockCurrentCustomerService = new Mock<ICurrentCustomerService>();
        var fakeHistory = TicketHistory.CreateChanges(
            new Ticket
            {
                Id = 1,
                Status = TicketStatusEnum.Open,
                CreatedDate = new DateTime(2024, 1, 1),
                SeverityId = 2,
                TypeId = 3,
                CategoryId = 1,
                category = new Category { Name = "Original Category" },
                Type = new TicketType { Name = "Original Type" },
                Severity = new Severity { Name = "Original Severity" },
                TicketTitle = new TicketTitle { Name = "Original Title" }
            }, new Ticket
            {
                Id = 1,
                Status = TicketStatusEnum.InProgress, // changed
                CreatedDate = new DateTime(2024, 1, 1),
                SeverityId = 2,
                TypeId = 3,
                CategoryId = 2, // changed
                category = new Category { Name = "New Category" },
                Type = new TicketType { Name = "Original Type" },
                Severity = new Severity { Name = "Original Severity" },
                TicketTitle = new TicketTitle { Name = "Original Title" }
            }, 1, "John Doe");

        mockRepo.Setup(r => r.GetTicketHistoryById(ticketId))
                .ReturnsAsync(fakeHistory);

        var service = new TicketBusiness(mockRepo.Object, mockEmailSender.Object, mockCurrentCustomerService.Object, Substitute.For<ILogger<TicketBusiness>>(), Substitute.For<ITicketTitleRepository>());

        // Act
        var result = (await service.FetchTicketHistory(ticketId)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Category", result[0].Title);
        Assert.Equal("Status Changed", result[1].Title);
        Assert.Equal("John Doe", result[0].ChangedByName);
    }

    [Fact]
    public async Task FetchTicketHistory_ShouldThrowApplicationException_OnRepoFailure()
    {
        // Arrange
        var ticketId = 999;
        var mockRepo = new Mock<ITicketRepository>();
        var mockEmailSender = new Mock<IEmailOperations>();
        var mockCurrentCustomerService = new Mock<ICurrentCustomerService>();

        mockRepo.Setup(r => r.GetTicketHistoryById(ticketId))
                .ThrowsAsync(new Exception("Database failed"));

        var service = new TicketBusiness(mockRepo.Object, mockEmailSender.Object, mockCurrentCustomerService.Object, Substitute.For<ILogger<TicketBusiness>>(), Substitute.For<ITicketTitleRepository>());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApplicationException>(() => service.FetchTicketHistory(ticketId));

        Assert.Contains($"Error retrieving ticket history for ticket {ticketId}", ex.Message);
    }
}
