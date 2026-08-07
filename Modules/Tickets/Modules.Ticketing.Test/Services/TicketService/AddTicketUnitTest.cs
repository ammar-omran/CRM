using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.JWT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Application.DTOs;
using TicketManagement.Application.Interfaces.Repositories;
using TicketManagement.Application.Services;
using TicketManagement.Domain.OrganizationAggregate.
using Xunit;

namespace TicketManagement.Test.Services.TicketService
{
    public class AddTicketUnitTest
    {
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IEmailOperations> _mockEmailOperations;
        private readonly Mock<ICurrentCustomerService> _mockCurrentCustomerService;
        private readonly Mock<ITicketTitleRepository> _mockTicketTitleRepo;
        private readonly TicketBusiness _ticketBusiness;

        public AddTicketUnitTest()
        {
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockEmailOperations = new Mock<IEmailOperations>();
            _mockCurrentCustomerService = new Mock<ICurrentCustomerService>();
            _mockTicketTitleRepo = new Mock<ITicketTitleRepository>();

            _ticketBusiness = new TicketBusiness(
                _mockTicketRepo.Object,
                _mockEmailOperations.Object,
                _mockCurrentCustomerService.Object,
                Substitute.For<ILogger<TicketBusiness>>(),
                _mockTicketTitleRepo.Object
            );
        }

        [Fact]
        public async Task AddTicketAsync_WithPredefinedTitle_MapsDefaultSeverity()
        {
            // Arrange
            var requestDto = new TicketDTO
            {
                CategoryId = 1,
                TypeId = 1,
                TitleId = 1, 
                Title = "",
                Description = "Failed Recharge",
                CustomerId = -1,
                UserId = 1,
                UserName = "TestUser",
                CustomerEmail = "test@company.com",
                attachments = new List<TicketAttachmentDTO>()
            };

            var predefinedTitle = new TicketTitle
            {
                Id = 1,
                Name = "Failed Recharge",
                CategoryId = 1,
                DefaultSeverityId = 4 
            };

            // Setup repository to return predefined title mappings
            _mockTicketTitleRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(predefinedTitle);

            // Capture the created ticket before adding to database
            Ticket savedTicket = null;
            _mockTicketRepo.Setup(r => r.AddTicketAsync(It.IsAny<Ticket>()))
                .Callback<Ticket>(ticket => {
                    ticket.Id = 101;
                    savedTicket = ticket;
                });

            // Act
            var result = await _ticketBusiness.AddTicketAsync(requestDto);

            // Assert
            Assert.NotEqual(0, result);
            Assert.NotNull(savedTicket);
            Assert.Equal("Failed Recharge", savedTicket.Title);
            Assert.Equal(4, savedTicket.SeverityId); // Matches DefaultSeverityId
        }

        [Fact]
        public async Task AddTicketAsync_WithOtherTitle_LeavesSeverityNull()
        {
            // Arrange
            var requestDto = new TicketDTO
            {
                CategoryId = 1,
                TypeId = 1,
                TitleId = 18, // Represents "Other"
                Title = "Custom Customization Requested",
                Description = "I need this custom feature.",
                CustomerId = 1,
                UserId = 1,
                UserName = "TestUser",
                CustomerEmail = "test@company.com",
                attachments = new List<TicketAttachmentDTO>()
            };

            var otherTitle = new TicketTitle
            {
                Id = 18,
                Name = "Other",
                CategoryId = 1,
                DefaultSeverityId = null
            };

            // Setup repository to return other title mappings
            _mockTicketTitleRepo.Setup(r => r.GetByIdAsync(4))
                .ReturnsAsync(otherTitle);

            // Capture the created ticket before adding to database
            Ticket savedTicket = null;
            _mockTicketRepo.Setup(r => r.AddTicketAsync(It.IsAny<Ticket>()))
                .Callback<Ticket>(ticket => {
                    ticket.Id = 202;
                    savedTicket = ticket;
                });

            // Act
            var result = await _ticketBusiness.AddTicketAsync(requestDto);

            // Assert
            Assert.NotEqual(0, result);
            Assert.NotNull(savedTicket);
            // Verify custom title supplied by user is preserved
            Assert.Equal("Custom Customization Requested", savedTicket.Title);
            // Assert SeverityId remains Null since it doesn't automatically map
            Assert.Null(savedTicket.SeverityId);
        }
    }
}
