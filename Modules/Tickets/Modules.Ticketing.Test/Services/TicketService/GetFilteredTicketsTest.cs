using TicketManagement.Domain.Entities;
using TicketManagement.Application.Services;
using TicketManagement.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using SharedKernel.Logging;
using SharedKernel.JWT;
using TicketManagement.Application.DTOs;
using TicketManagement.Application.Interfaces.Repositories;

namespace TicketManagement.Test.Services.TicketService;

public class GetFilteredTicketsTest : IAsyncDisposable
{
    private readonly TicketRepository _ticketRepo;
    private readonly TicketManagementContext _context;
    private readonly TicketBusiness _service;

    public GetFilteredTicketsTest()
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
        string subject = "Test Subject",
        string description = "Test Description",
        int customerId = 1,
        TicketStatusEnum status = TicketStatusEnum.Open,
        int severityId = 1,
        int supportLineNum = 1,
        DateTime? createdDate = null) => new()
        {
            CustomerId = customerId,
            Title = subject,
            Description = description,
            Status = status,
            SeverityId = severityId,
            // SupportLine = supportLineNum, no field for this in the entity, but we can use it in filters
            CreatedDate = createdDate ?? DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            CreatedByName = "Agent A",
            CustomerName = "TestCustomer",
            CustomerEmail = "TestCustomerEmail",
            UpdatedByName = "Admin",
            Severity = new Severity { Name = "Low" }
        };


    #region Title and Projection Tests
    [Fact]
    public async Task GetFilteredTickets_ShouldProjectCorrectly_WhenTicketExists()
    {
        // Arrange
        var ticket = MakeTicket("Projection Test", "Some description");
        await SeedTicketsAsync([ticket]);

        var filters = new TicketFilters { Skip = 0, Limit = 10 };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Value.Items);
        Assert.Equal("Projection Test", item.Title);
        Assert.Equal("Some description", item.Description);
        Assert.Equal("Low", item.Severity);
        Assert.Equal(TicketStatusEnum.Open.ToString(), item.Status);
        Assert.Equal("Agent A", item.AssignedTo);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldReturnEmpty_WhenNoTicketsExist()
    {
        // Arrange
        var filters = new TicketFilters { Skip = 0, Limit = 10 };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldReturnAllTickets_WhenNoFiltersApplied()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket(customerId: 1),
            MakeTicket(customerId: 2),
            MakeTicket(customerId: 3),
        ]);

        var filters = new TicketFilters { Skip = 0, Limit = 10 };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldFilterByTitle_WhenTitleProvided()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket("Login crash"),
            MakeTicket("Payment failure"),
        ]);

        var filters = new TicketFilters { Skip = 0, Limit = 10, Title = "Login" };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Contains("Login", result.Value.Items[0].Title);
    }
    #endregion

    #region Status Tests
    [Fact]
    public async Task GetFilteredTickets_ShouldFilterByStatus_WhenStatusIdProvided()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket(status: TicketStatusEnum.Open),
            MakeTicket(status: TicketStatusEnum.Closed),
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            StatusIds = [(int)TicketStatusEnum.Open]
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(TicketStatusEnum.Open.ToString(), result.Value.Items[0].Status);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldReturnTicketsWithMultipleStatuses_WhenMultipleStatusIdsProvided()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket(status: TicketStatusEnum.Open),
        MakeTicket(status: TicketStatusEnum.InProgress),
        MakeTicket(status: TicketStatusEnum.Closed),
    ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            StatusIds = [(int)TicketStatusEnum.Open, (int)TicketStatusEnum.InProgress]
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItemsCount);
        Assert.DoesNotContain(result.Value.Items, t => t.Status == TicketStatusEnum.Closed.ToString());
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldReturnAllTickets_WhenAllStatusIdsProvided()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket(status: TicketStatusEnum.Open),
        MakeTicket(status: TicketStatusEnum.InProgress),
        MakeTicket(status: TicketStatusEnum.Closed),
    ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            StatusIds =
            [
                (int)TicketStatusEnum.Open,
            (int)TicketStatusEnum.InProgress,
            (int)TicketStatusEnum.Closed
            ]
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldReturnEmpty_WhenNoTicketMatchesAnyStatusId()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket(status: TicketStatusEnum.Closed),
    ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            StatusIds = [(int)TicketStatusEnum.Open, (int)TicketStatusEnum.InProgress]
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }
    #endregion

    #region Severity Tests
    [Fact]
    public async Task GetFilteredTickets_ShouldFilterBySeverity_WhenSeverityIdProvided()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket(severityId: 1),
            MakeTicket(severityId: 2),
        ]);

        var filters = new TicketFilters { Skip = 0, Limit = 10, SeverityIds = [2] };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldReturnTicketsWithMultipleSeverities_WhenMultipleSeverityIdsProvided()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket(severityId: 1),
            MakeTicket(severityId: 2),
            MakeTicket(severityId: 3),
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            SeverityIds = [1, 2]
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldReturnEmpty_WhenNoTicketMatchesAnySeverityId()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket(severityId: 1),
            MakeTicket(severityId: 2),
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            SeverityIds = [3, 4]
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }
    #endregion

    #region SupportLine Tests
    [Fact]
    public async Task GetFilteredTickets_ShouldFilterBySupportLine_WhenSupportLineNumProvided()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket(supportLineNum: 1),
            MakeTicket(supportLineNum: 2),
            MakeTicket(supportLineNum: 1),
        ]);

        var filters = new TicketFilters { 
            Skip = 0,
            Limit = 10,
            SupportLineNums = [1] 
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldReturnTicketsWithMultipleSupportLines_WhenMultipleSupportLineNumsProvided()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket(supportLineNum: 1),
            MakeTicket(supportLineNum: 2),
            MakeTicket(supportLineNum: 3),
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            SupportLineNums = [1, 2]
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldReturnEmpty_WhenNoTicketMatchesAnySupportLineNum()
    {
        // Arrange
        await SeedTicketsAsync([
            MakeTicket(supportLineNum: 1),
            MakeTicket(supportLineNum: 2),
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            SupportLineNums = [3, 4]
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }
    #endregion

    #region Date Filters Tests
    [Fact]
    public async Task GetFilteredTickets_ShouldFilterByFromDate_WhenFromDateProvided()
    {
        // Arrange
        var baseDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await SeedTicketsAsync([
            MakeTicket(createdDate: baseDate.AddDays(-10)), // before range
            MakeTicket(createdDate: baseDate),              // on boundary
            MakeTicket(createdDate: baseDate.AddDays(5)),   // after
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            FromDate = baseDate
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldFilterByToDate_WhenToDateProvided()
    {
        // Arrange
        var baseDate = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        await SeedTicketsAsync([
            MakeTicket(createdDate: baseDate.AddDays(-5)), // before
            MakeTicket(createdDate: baseDate),             // on boundary
            MakeTicket(createdDate: baseDate.AddDays(5)), // after range
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            ToDate = baseDate
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldFilterByDateRange_WhenBothDatesProvided()
    {
        // Arrange
        var from = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        await SeedTicketsAsync([
            MakeTicket(createdDate: from.AddDays(-1)), // before range
            MakeTicket(createdDate: from),             // on from boundary
            MakeTicket(createdDate: from.AddDays(5)), // inside range
            MakeTicket(createdDate: to),               // on to boundary
            MakeTicket(createdDate: to.AddDays(1)),   // after range
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            FromDate = from,
            ToDate = to
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldReturnEmpty_WhenDateRangeMatchesNothing()
    {
        // Arrange
        var baseDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        await SeedTicketsAsync([MakeTicket(createdDate: baseDate)]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            FromDate = baseDate.AddDays(10),
            ToDate = baseDate.AddDays(20)
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }
    #endregion

    #region Compined Filters Tests
    [Fact]
    public async Task GetFilteredTickets_ShouldApplyAllFilters_WhenAllFiltersProvided()
    {
        // Arrange
        var targetDate = new DateTime(2025, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        await SeedTicketsAsync([
            // matches all filters
            MakeTicket(subject: "Login crash", status: TicketStatusEnum.Open,
                severityId: 1, supportLineNum: 1, createdDate: targetDate),
            // wrong title
            MakeTicket(subject: "Payment fail", status: TicketStatusEnum.Open,
                severityId: 1, supportLineNum: 1, createdDate: targetDate),
            // wrong status
            MakeTicket(subject: "Login crash", status: TicketStatusEnum.Closed,
                severityId: 1, supportLineNum: 1, createdDate: targetDate),
            // wrong date
            MakeTicket(subject: "Login crash", status: TicketStatusEnum.Open,
                severityId: 1, supportLineNum: 1, createdDate: targetDate.AddMonths(-1)),
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            Title = "Login",
            StatusIds = [(int)TicketStatusEnum.Open],
            SeverityIds = [1],
            SupportLineNums = [1],
            FromDate = targetDate,
            ToDate = targetDate.AddDays(2)
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(1, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldIntersectFilters_WhenMultipleMultiSelectFiltersProvided()
    {
        // Arrange
        await SeedTicketsAsync([
            // ✅ matches all
            MakeTicket(status: TicketStatusEnum.Open, severityId: 1, supportLineNum: 1),
            // ❌ wrong severity
            MakeTicket(status: TicketStatusEnum.Open, severityId: 3, supportLineNum: 1),
            // ❌ wrong support line
            MakeTicket(status: TicketStatusEnum.InProgress, severityId: 2, supportLineNum: 3),
            // ✅ matches all
            MakeTicket(status: TicketStatusEnum.InProgress, severityId: 2, supportLineNum: 2),
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            StatusIds = [(int)TicketStatusEnum.Open, (int)TicketStatusEnum.InProgress],
            SeverityIds = [1, 2],
            SupportLineNums = [1, 2]
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldCombineMultiSelectWithDateRange_WhenAllFiltersProvided()
    {
        // Arrange
        var targetDate = new DateTime(2025, 3, 10, 0, 0, 0, DateTimeKind.Utc);

        await SeedTicketsAsync([
            // ✅ matches all
            MakeTicket(status: TicketStatusEnum.Open, severityId: 1, createdDate: targetDate),
            // ❌ outside date range
            MakeTicket(status: TicketStatusEnum.Open, severityId: 1, createdDate: targetDate.AddMonths(-3)),
            // ❌ wrong status
            MakeTicket(status: TicketStatusEnum.Closed, severityId: 1, createdDate: targetDate),
            // ✅ matches all
            MakeTicket(status: TicketStatusEnum.InProgress, severityId: 2, createdDate: targetDate),
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            StatusIds = [(int)TicketStatusEnum.Open, (int)TicketStatusEnum.InProgress],
            SeverityIds = [1, 2],
            FromDate = targetDate,
            ToDate = targetDate.AddDays(1)
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItemsCount);
    }

    [Fact]
    public async Task GetFilteredTickets_ShouldCombineMultiSelectWithTitle_WhenAllFiltersProvided()
    {
        // Arrange
        await SeedTicketsAsync([
            // ✅ matches all
            MakeTicket(subject: "Login crash", status: TicketStatusEnum.Open, severityId: 1),
            // ❌ wrong title
            MakeTicket(subject: "Payment fail", status: TicketStatusEnum.Open, severityId: 1),
            // ❌ wrong severity
            MakeTicket(subject: "Login timeout", status: TicketStatusEnum.InProgress, severityId: 3),
            // ✅ matches all
            MakeTicket(subject: "Login timeout", status: TicketStatusEnum.InProgress, severityId: 2),
        ]);

        var filters = new TicketFilters
        {
            Skip = 0,
            Limit = 10,
            Title = "Login",
            StatusIds = [(int)TicketStatusEnum.Open, (int)TicketStatusEnum.InProgress],
            SeverityIds = [1, 2],
        };

        // Act
        var result = await _service.GetFilteredTickets(filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItemsCount);
    }
    #endregion

}
