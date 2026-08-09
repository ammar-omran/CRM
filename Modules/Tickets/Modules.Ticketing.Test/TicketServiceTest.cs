using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.JWT;
using TicketManagement.Application.DTOs;
using TicketManagement.Features.GetFilteredTickets;
using TicketManagement.Application.Interfaces.Repositories;
using TicketManagement.Application.Services;

namespace Modules.Ticketing.Test;

public class TicketServiceTests // 🛠️ Fixed class name and made it public
{
    private readonly Mock<ITicketRepository> _mockRepo;
    private readonly Mock<IEmailOperations> _mockEmailSender;
    private readonly TicketBusiness _ticketService;

    public TicketServiceTests() // 🛠️ Constructor must match class name
    {
        _mockRepo = new Mock<ITicketRepository>();
        _mockEmailSender = new Mock<IEmailOperations>();
        _ticketService = new TicketBusiness(_mockRepo.Object, _mockEmailSender.Object, new Mock<ICurrentCustomerService>().Object, Substitute.For<ILogger<TicketBusiness>>(), Substitute.For<ITicketTitleRepository>());
    }
}
