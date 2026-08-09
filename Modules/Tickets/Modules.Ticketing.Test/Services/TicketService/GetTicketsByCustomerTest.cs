using Modules.Ticketing.Domain.Entities;
using TicketManagement.Application.Services;
using TicketManagement.Infrastructure.Repositories;
using SharedKernel.JWT;
using SharedKernel.Logging;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.DTOs;
using TicketManagement.Application.Interfaces.Repositories;

namespace Modules.Ticketing.Test.Services.TicketService;
public class GetTicketsByCustomerTest : IAsyncDisposable
{
    private readonly TicketRepository _ticketRepo;
    private readonly TicketManagementContext _context;
    private readonly TicketBusiness _service;

    public GetTicketsByCustomerTest()
    {
        var options = new DbContextOptionsBuilder<TicketManagementContext>()
            .UseInMemoryDatabase(databaseName: $"TicketsDb_{Guid.NewGuid()}")
            .Options;

        _context = new TicketManagementContext(options);
        _ticketRepo = new TicketRepository(_context, Substitute.For<ILogHelper>());
        _service = new TicketBusiness(
            _ticketRepo,
            Substitute.For<IEmailOperations>(),
            Substitute.For<ICurrentCustomerService>(),
            Substitute.For<ILogger<TicketBusiness>>(),
            Substitute.For<ITicketTitleRepository>()
            );
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }


    private async Task SeedTicketsAsync(IEnumerable<Ticket> tickets)
    {
        await _ticketRepo.AddRangeAsync(tickets);
    }

    private static Ticket MakeTicket(
        int customerId,
        string subject = "Test Subject",
        string description = "Test Description",
        TicketStatusEnum status = TicketStatusEnum.Open,
        int severityId = 1) => new()
        {
            CustomerId = customerId,
            Title = subject,
            Description = description,
            Status = status,
            SeverityId = severityId,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            CreatedByName = "Agent A",
            CustomerName = "TestCustomer",
            CustomerEmail = "TestCustomerEmail",
            UpdatedByName = "Admin",
            Severity = new Severity { Name = "Low" }
        };


    [Fact]
    public async Task GetTicketsByCustomer_ShouldReturnTickets_WhenCustomerHasTickets()
    {
        // Arrange
        const int customerId = 1;
        await SeedTicketsAsync([
            MakeTicket(customerId, "Issue A"),
            MakeTicket(customerId, "Issue B"),
            MakeTicket(customerId: 99, "Other customer") // should be excluded
        ]);

        var filters = new CustomerTicketFilters { Limit = 10, Skip = 0 };

        // Act
        var result = await _service.GetTicketsByCustomer(filters, customerId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItemsCount);
        Assert.All(result.Value.Items, t => Assert.NotNull(t.Title));
    }

    [Fact]
    public async Task GetTicketsByCustomer_ShouldReturnEmpty_WhenCustomerHasNoTickets()
    {
        // Arrange
        await SeedTicketsAsync([MakeTicket(customerId: 99)]);

        var filters = new CustomerTicketFilters { Limit = 10, Skip = 0 };

        // Act
        var result = await _service.GetTicketsByCustomer(filters, customerId: 1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetTicketsByCustomer_ShouldFilterByTitle_WhenTitleProvided()
    {
        // Arrange
        const int customerId = 1;
        await SeedTicketsAsync([
            MakeTicket(customerId, "Login crash"),
            MakeTicket(customerId, "Payment failure"),
        ]);

        var filters = new CustomerTicketFilters { Limit = 10, Skip = 0, Title = "Login" };

        // Act
        var result = await _service.GetTicketsByCustomer(filters, customerId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Contains("Login", result.Value.Items[0].Title);
    }

    [Fact]
    public async Task GetTicketsByCustomer_ShouldFilterByStatus_WhenStatusIdProvided()
    {
        // Arrange
        const int customerId = 1;
        await SeedTicketsAsync([
            MakeTicket(customerId, status: TicketStatusEnum.Open),
            MakeTicket(customerId, status: TicketStatusEnum.Closed),
        ]);

        var filters = new CustomerTicketFilters
        {
            Limit = 10,
            Skip = 0,
            StatusId = (int)TicketStatusEnum.Open
        };

        // Act
        var result = await _service.GetTicketsByCustomer(filters, customerId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(TicketStatusEnum.Open.ToString(), result.Value.Items[0].Status);
    }

    [Fact]
    public async Task GetTicketsByCustomer_ShouldPaginate_WhenPageSizeIsSmall()
    {
        // Arrange
        const int customerId = 1;
        await SeedTicketsAsync(Enumerable.Range(1, 10).Select(_ => MakeTicket(customerId)));

        var filters = new CustomerTicketFilters { Limit = 3, Skip = 3 };

        // Act
        var result = await _service.GetTicketsByCustomer(filters, customerId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetTicketsByCustomer_ShouldSkipTotalCount_WhenSkipTotalIsTrue()
    {
        // Arrange
        const int customerId = 1;
        await SeedTicketsAsync([MakeTicket(customerId)]);

        var filters = new CustomerTicketFilters { Limit = 10, Skip = 0, SkipTotal = true };

        // Act
        var result = await _service.GetTicketsByCustomer(filters, customerId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(-1, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetTicketsByCustomer_ShouldProjectCorrectly_WhenTicketExists()
    {
        // Arrange
        const int customerId = 1;
        var ticket = MakeTicket(customerId, "Projection Test", "Some description");
        await SeedTicketsAsync([ticket]);

        var filters = new CustomerTicketFilters { Limit = 10, Skip = 0 };

        // Act
        var result = await _service.GetTicketsByCustomer(filters, customerId, CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Value.Items);
        Assert.Equal("Projection Test", item.Title);
        Assert.Equal("Some description", item.Description);
        Assert.Equal("Low", item.Severity);
        Assert.Equal(TicketStatusEnum.Open.ToString(), item.Status);
        Assert.Equal("Agent A", item.AssignedTo);
    }
}
