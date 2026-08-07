using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.JWT;
using TicketManagement.Application.DTOs;
using TicketManagement.Application.Interfaces.Repositories;
using TicketManagement.Application.Services;
using TicketManagement.Domain.Entities;
using Xunit;

namespace TicketManagement.Test.Services.TicketService
{
    public class AddCommentUnitTest
    {
        private static (TicketBusiness sut, Mock<ITicketRepository> repo, Mock<IEmailOperations> email, Mock<ICurrentCustomerService> current) CreateSut(
            int currentCustomerId = 10,
            string currentCustomerName = "Alice",
            string currentCustomerEmail = "alice@example.com")
        {
            var repoMock = new Mock<ITicketRepository>(MockBehavior.Strict);
            var emailMock = new Mock<IEmailOperations>(MockBehavior.Loose);
            var currentMock = new Mock<ICurrentCustomerService>(MockBehavior.Strict);
            var titleRepoMock = new Mock<ITicketTitleRepository>(MockBehavior.Strict);
            currentMock.SetupGet(c => c.CustomerId).Returns(currentCustomerId);
            currentMock.SetupGet(c => c.CustomerName).Returns(currentCustomerName);
            currentMock.SetupGet(c => c.CustomerEmail).Returns(currentCustomerEmail);

            var sut = new TicketBusiness(repoMock.Object, emailMock.Object, currentMock.Object, Substitute.For<ILogger<TicketBusiness>>(), titleRepoMock.Object);
            return (sut, repoMock, emailMock, currentMock);
        }

        [Fact(DisplayName = "Adding a comment succeeds for the current customer's ticket")]
        public async Task AddCommentAsync_Success_OwnTicket_ReturnsTrue_AndCallsRepoAndEmail()
        {
            // Arrange
            var (sut, repo, email, _) = CreateSut();
            var dto = new AddCommentDTO { TicketId = 42, Content = "Test comment" };

            repo.Setup(r => r.GetTicketEntityById(dto.TicketId))
                .ReturnsAsync(new Ticket { Id = dto.TicketId, CustomerId = 10, CustomerEmail = "alice@example.com" });

            repo.Setup(r => r.AddCommentAsync(It.IsAny<TicketComment>()))
                .ReturnsAsync(1);

            // Act
            var result = await sut.AddCommentAsync(dto);

            // Assert
            Assert.True(result>0);
            repo.Verify(r => r.GetTicketEntityById(dto.TicketId), Times.Once);
            repo.Verify(r => r.AddCommentAsync(It.Is<TicketComment>(c =>
                c.TicketId == dto.TicketId &&
                c.Description == dto.Content &&
                c.CreatedBy == 10 &&
                c.CreatedByName == "Alice"
            )), Times.Once);
            email.Verify(e => e.SendEmailAsync(
                "alice@example.com",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ), Times.Once);
        }





        [Fact(DisplayName = "Email sending failure does not affect AddCommentAsync result")]
        public async Task AddCommentAsync_EmailSendingFails_StillReturnsTrue_AndContinues()
        {
            // Arrange
            var (sut, repo, email, _) = CreateSut();
            var dto = new AddCommentDTO { TicketId = 55, Content = "With email failure" };

            repo.Setup(r => r.GetTicketEntityById(dto.TicketId))
                .ReturnsAsync(new Ticket { Id = dto.TicketId, CustomerId = 10, CustomerEmail = "alice@example.com" });

            repo.Setup(r => r.AddCommentAsync(It.IsAny<TicketComment>()))
                .ReturnsAsync(77);

            email.Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP error"));

            // Act
            var ok = await sut.AddCommentAsync(dto);

            // Assert
            Assert.True(ok>0);
            email.Verify(e => e.SendEmailAsync(
                It.Is<string>(to => to == "alice@example.com"),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ), Times.Once);
        }

        [Fact(DisplayName = "Adding a comment to a non-existent ticket throws KeyNotFoundException")]
        public async Task AddCommentAsync_TicketNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var (sut, repo, email, _) = CreateSut();
            var dto = new AddCommentDTO { TicketId = 404, Content = "Will fail" };

            repo.Setup(r => r.GetTicketEntityById(dto.TicketId))
                .ReturnsAsync((Ticket?)null);

            // Act
            await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.AddCommentAsync(dto));

            // Assert
            repo.Verify(r => r.GetTicketEntityById(dto.TicketId), Times.Once);
            repo.Verify(r => r.AddCommentAsync(It.IsAny<TicketComment>()), Times.Never);
            email.Verify(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ), Times.Never);
        }
    }
}
