using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Attachments;

public sealed record DeleteAttachmentRequest(int AttachmentId);

public interface IDeleteAttachmentHandler : IHandler
{
    Task<Result<Success>> HandleAsync(int attachmentId, CancellationToken cancellationToken = default);
    Task<Result<Success>> HandleAsync(DeleteAttachmentRequest request, CancellationToken cancellationToken = default);
}

internal sealed class DeleteAttachmentHandler(
    TicketingDbContext db,
    IConfiguration config,
    ILogger<DeleteAttachmentHandler> logger) : IDeleteAttachmentHandler
{
    public Task<Result<Success>> HandleAsync(int attachmentId, CancellationToken ct = default)
        => HandleAsync(new DeleteAttachmentRequest(attachmentId), ct);

    public async Task<Result<Success>> HandleAsync(DeleteAttachmentRequest request, CancellationToken ct = default)
    {
        var attachmentId = request.AttachmentId;
        string storagePath = config["FileStorage:Path"] ?? "";

        var attachment = await db.Attachments.FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
        if (attachment is null)
            return Error.NotFound("Attachment.NotFound", $"Attachment with id '{attachmentId}' Not Found!");

        if (string.IsNullOrWhiteSpace(storagePath))
            return Error.Validation("Attachment.StoragePathMissing", "No storage path provided!");

        storagePath += @$"{attachment.Type}";
        var filePath = Path.Combine(storagePath, $"{attachment.Id}{Path.GetExtension(attachment.FileName)}");
        if (!File.Exists(filePath))
            return Error.NotFound("Attachment.FileNotFound", $"File for attachment '{attachmentId}' not found on server.");

        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete file {Path}", filePath);
            return Error.Failure("Attachment.StorageFailure", ex.Message);
        }

        try
        {
            db.Attachments.Remove(attachment);
            var saved = await db.SaveChangesAsync(ct) > 0;
            if (!saved)
                return Error.Failure("Attachment.DbError", $"Failed to delete attachment '{attachment.Id}'.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing attachment {Id} from DB", attachmentId);
            return Error.Failure("Attachment.DbError", ex.Message);
        }

        logger.LogInformation("Attachment '{Id}' deleted successfully.", attachment.Id);
        return Result.Success;
    }
}

public class DeleteAttachmentRequestValidator : AbstractValidator<DeleteAttachmentRequest>
{
    public DeleteAttachmentRequestValidator()
    {
        RuleFor(x => x.AttachmentId).GreaterThan(0).WithMessage("AttachmentId must be greater than 0");
    }
}
