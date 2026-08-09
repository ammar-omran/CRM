using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using TicketManagement.Application.Configurations;
using TicketManagement.Application.DTOs;
using TicketManagement.Application.Interfaces.Repositories;
using TicketManagement.Application.Services;
using Modules.Ticketing.Domain.Entities;
using Xunit;

namespace Modules.Ticketing.Test.Services.AttachmentService
{
    public class AttachmentsServiceTests
    {
        private readonly Mock<IAttachmentRepository> _mockAttachRepo;
        private readonly AttachmentsService _service;

        public AttachmentsServiceTests()
        {
            _mockAttachRepo = new Mock<IAttachmentRepository>();
            _service = new AttachmentsService(_mockAttachRepo.Object);
        }

        private IFormFile CreateMockFormFile(string fileName, long length, string content = "dummy content")
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream target, CancellationToken token) =>
                {
                    stream.Position = 0;
                    return stream.CopyToAsync(target, token);
                });
            return mockFile.Object;
        }

        [Fact]
        public void ValidateAttachment_WithValidFile_ShouldReturnSuccess()
        {
            // Arrange
            var configs = new AttachmentConfigs { MaxSizeMB = 5, AllowedExtensions = new[] { ".txt" } };
            var file = CreateMockFormFile("test.txt", 1024);

            // Act
            var result = _service.ValidateAttachment(file, configs);

            // Assert
            result.Status.Should().BeTrue();
            result.Message.Should().Contain("validated Successfully");
        }

        [Fact]
        public void ValidateAttachment_WithNullFile_ShouldReturnFailure()
        {
            // Arrange
            var configs = new AttachmentConfigs();

            // Act
            var result = _service.ValidateAttachment(null, configs);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Be("File is empty.");
        }

        [Fact]
        public void ValidateAttachment_WithTooLargeFile_ShouldReturnFailure()
        {
            // Arrange
            var configs = new AttachmentConfigs { MaxSizeMB = 1, AllowedExtensions = new[] { ".txt" } };
            var file = CreateMockFormFile("large.txt", 2 * 1024 * 1024); // 2MB

            // Act
            var result = _service.ValidateAttachment(file, configs);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Be("This file is too long. max limit is 1 mega byte.");
        }

        [Fact]
        public void ValidateAttachment_WithInvalidExtension_ShouldReturnFailure()
        {
            // Arrange
            var configs = new AttachmentConfigs { MaxSizeMB = 5, AllowedExtensions = new[] { ".jpg", ".png" } };
            var file = CreateMockFormFile("document.txt", 1024);

            // Act
            var result = _service.ValidateAttachment(file, configs);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Be("Unsupported file type. Please upload a file in .jpg, .png format");
        }

        [Fact]
        public async Task AddAttachmentToDB_WithValidDto_ShouldReturnSuccessAndId()
        {
            // Arrange
            var dto = new AttachmentDTO { FileName = "test.txt", FileType = "text/plain" };
            var savedAttachment = new Attachment { Id = 123, FileName = "test.txt", FileType = "text/plain" };
            _mockAttachRepo.Setup(r => r.AddAttachment(It.IsAny<Attachment>())).ReturnsAsync(savedAttachment);

            // Act
            var (ack, fileId) = await _service.AddAttachmentToDB(dto);

            // Assert
            ack.Status.Should().BeTrue();
            fileId.Should().Be(123);
            ack.Message.Should().Contain("uploaded successfully");
        }

        [Fact]
        public async Task AddAttachmentToDB_WhenRepoFails_ShouldReturnFailure()
        {
            // Arrange
            var dto = new AttachmentDTO { FileName = "test.txt" };
            _mockAttachRepo.Setup(r => r.AddAttachment(It.IsAny<Attachment>())).ReturnsAsync(new Attachment { Id = 0 });

            // Act
            var (ack, fileId) = await _service.AddAttachmentToDB(dto);

            // Assert
            ack.Status.Should().BeFalse();
            fileId.Should().Be(0);
            ack.Message.Should().Be("Failed to save attachment to the database.");
        }

        [Fact]
        public async Task UpdateAttachmentReference_WhenAttachmentExists_ShouldReturnSuccess()
        {
            // Arrange
            var attachmentId = 1;
            var referenceId = 100;
            var attachment = new Attachment { Id = attachmentId };
            _mockAttachRepo.Setup(r => r.GetAttachmentById(attachmentId)).ReturnsAsync(attachment);
            _mockAttachRepo.Setup(r => r.UpdateAttachment(It.IsAny<Attachment>())).ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAttachmentReference(attachmentId, referenceId);

            // Assert
            result.Status.Should().BeTrue();
            result.Code.Should().Be(AcknowledgementStatus.Success);
            result.Message.Should().Contain("updated successfully");
            _mockAttachRepo.Verify(r => r.UpdateAttachment(It.Is<Attachment>(a => a.ReferenceId == referenceId)), Times.Once);
        }

        [Fact]
        public async Task UpdateAttachmentReference_WhenAttachmentNotFound_ShouldReturnNoDataFound()
        {
            // Arrange
            var attachmentId = 1;
            _mockAttachRepo.Setup(r => r.GetAttachmentById(attachmentId)).ReturnsAsync((Attachment)null);

            // Act
            var result = await _service.UpdateAttachmentReference(attachmentId, 100);

            // Assert
            result.Status.Should().BeFalse();
            result.Code.Should().Be(AcknowledgementStatus.NoDataFound);
            result.Message.Should().Be($"Attachment with id '{attachmentId}' Not Found!");
        }

        [Fact]
        public async Task GetAttachmentFile_WhenAttachmentNotFoundInDb_ShouldReturnFailure()
        {
            // Arrange
            var attachmentId = 1;
            _mockAttachRepo.Setup(r => r.GetAttachmentById(attachmentId)).ReturnsAsync((Attachment)null);

            // Act
            var (ack, filePath, contentType) = await _service.GetAttachmentFile(attachmentId, "C:\\temp");

            // Assert
            ack.Status.Should().BeFalse();
            filePath.Should().BeEmpty();
            contentType.Should().BeEmpty();
            ack.Message.Should().Be($"Attachment with id '{attachmentId}' not found.");
        }

        // Note: Testing the success path of `GetAttachmentFile` and `SaveAttachment` is challenging
        // for unit tests because they directly interact with the static `File` and `Directory` classes.
        // To properly unit test this, you would typically abstract file system operations behind an
        // interface (e.g., IFileSystem) and mock that interface. The current tests cover the logic
        // that doesn't depend on the actual file system.

        [Fact]
        public async Task FetchAttachmentsOfReference_WhenAttachmentsExist_ShouldReturnSuccessAndData()
        {
            // Arrange
            var referenceId = 1;
            var attachments = new List<Attachment>
            {
                new Attachment { Id = 1, FileName = "file1.pdf", FileType = "application/pdf", ReferenceId = referenceId, Type = AttachmentTypeEnum.Ticket },
                new Attachment { Id = 2, FileName = "file2.jpg", FileType = "image/jpeg", ReferenceId = referenceId, Type = AttachmentTypeEnum.Ticket }
            };
            _mockAttachRepo.Setup(r => r.GetAttachmentsByReferenceId(referenceId)).ReturnsAsync(attachments);

            // Act
            var (ack, data) = await _service.FetchAttachmentsOfReference(referenceId);

            // Assert
            ack.Status.Should().BeTrue();
            ack.Code.Should().Be(AcknowledgementStatus.Success);
            ack.Message.Should().Be("Attachments fetched successfully.");
            data.Should().NotBeNull();
            data.Should().HaveCount(2);
            data.First().FileId.Should().Be(1);
            data.First().FileName.Should().Be("file1.pdf");
        }

        [Fact]
        public async Task FetchAttachmentsOfReference_WhenNoAttachmentsExist_ShouldReturnNoDataFound()
        {
            // Arrange
            var referenceId = 99;
            _mockAttachRepo.Setup(r => r.GetAttachmentsByReferenceId(referenceId)).ReturnsAsync(new List<Attachment>());

            // Act
            var (ack, data) = await _service.FetchAttachmentsOfReference(referenceId);

            // Assert
            ack.Status.Should().BeFalse();
            ack.Code.Should().Be(AcknowledgementStatus.NoDataFound);
            ack.Message.Should().Be($"No attachments found for reference ID '{referenceId}'.");
            data.Should().BeEmpty();
        }

        [Fact]
        public async Task FetchAttachmentsOfReference_WhenRepositoryReturnsNull_ShouldReturnNoDataFound()
        {
            // Arrange
            var referenceId = 99;
            _mockAttachRepo.Setup(r => r.GetAttachmentsByReferenceId(referenceId)).ReturnsAsync((List<Attachment>)null);

            // Act
            var (ack, data) = await _service.FetchAttachmentsOfReference(referenceId);

            // Assert
            ack.Status.Should().BeFalse();
            ack.Code.Should().Be(AcknowledgementStatus.NoDataFound);
            ack.Message.Should().Be($"No attachments found for reference ID '{referenceId}'.");
            data.Should().BeEmpty();
        }
    }
}
