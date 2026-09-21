using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Domain.Entities;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Attachments;

public sealed record UploadAttachmentRequest(AttachmentTypeEnum Type, int? ReferenceId, IFormFile File);
public sealed record UploadAttachmentResponse(int FileId, string FileName, string FileType, string AttachmentFor, int? ReferenceId);

public interface IUploadAttachmentHandler : IHandler
{
    Task<Result<UploadAttachmentResponse>> HandleAsync(UploadAttachmentRequest request, CancellationToken cancellationToken = default);
}

internal sealed class UploadAttachmentHandler(
    TicketingDbContext db,
    IConfiguration config,
    ILogger<UploadAttachmentHandler> logger) : IUploadAttachmentHandler
{
    public async Task<Result<UploadAttachmentResponse>> HandleAsync(UploadAttachmentRequest request, CancellationToken ct = default)
    {
        var type = request.Type;
        var referenceId = request.ReferenceId;
        var file = request.File;

        // --- ValidateAttachment (ported from AttachmentsService.ValidateAttachment) ---
        var attachmentConfigs = config.GetSection("AttachmentsConfigurations")
            .Get<AttachmentConfigs[]>()?
            .FirstOrDefault(c => c.AttachmentFor == type.ToString());

        if (attachmentConfigs is null)
            return Error.Validation("Attachment.InvalidType", "Invalid Attachment Type");

        if (file == null || file.Length == 0)
            return Error.Validation("Attachment.FileRequired", "File is empty.");

        var maxSize = attachmentConfigs.MaxSizeMB * 1024 * 1024;
        if (file.Length > maxSize)
            return Error.Validation("Attachment.FileTooLarge", $"This file is too long. max limit is {attachmentConfigs.MaxSizeMB} mega byte.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!attachmentConfigs.AllowedExtensions.Contains(extension))
            return Error.Validation("Attachment.UnsupportedExtension", $"Unsupported file type. Please upload a file in {string.Join(", ", attachmentConfigs.AllowedExtensions)} format");

        // --- AddAttachmentToDB (ported from AttachmentsService.AddAttachmentToDB) ---
        var attachment = new Attachment
        {
            Id = 0,
            FileName = file.FileName,
            FileType = file.ContentType,
            Type = type,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            db.Attachments.Add(attachment);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while adding attachment to DB");
            return Error.Failure("Attachment.DbError", ex.Message);
        }

        if (attachment.Id == 0)
            return Error.Failure("Attachment.DbError", "Failed to save attachment to the database.");

        // --- SaveAttachment (ported from AttachmentsService.SaveAttachment) ---
        string storePath = config["FileStorage:Path"] ?? "";
        storePath += @$"{type}";

        if (string.IsNullOrEmpty(storePath))
            return Error.Validation("Attachment.StoragePathMissing", "No Storage path provided!");

        try
        {
            Directory.CreateDirectory(storePath);
            var uniqueFileName = $"{attachment.Id}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(storePath, uniqueFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving file for attachment {Id}", attachment.Id);
            try
            {
                db.Attachments.Remove(attachment);
                await db.SaveChangesAsync(ct);
            }
            catch { }
            return Error.Failure("Attachment.StorageFailure", $"Error saving file: {ex.Message}.");
        }

        logger.LogInformation("Attachment {FileName} uploaded successfully with ID: {Id}", attachment.FileName, attachment.Id);

        return new UploadAttachmentResponse(
            attachment.Id,
            attachment.FileName,
            attachment.FileType,
            type.ToString(),
            referenceId);
    }
}

public class UploadAttachmentRequestValidator : AbstractValidator<UploadAttachmentRequest>
{
    public UploadAttachmentRequestValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid Attachment Type");

        RuleFor(x => x.File)
            .NotNull().WithMessage("File is empty.");

        RuleFor(x => x.File.Length)
            .GreaterThan(0).WithMessage("File is empty.")
            .When(x => x.File != null);

        RuleFor(x => x.ReferenceId)
            .GreaterThanOrEqualTo(0).WithMessage("ReferenceId must be >= 0")
            .When(x => x.ReferenceId.HasValue);
    }
}
