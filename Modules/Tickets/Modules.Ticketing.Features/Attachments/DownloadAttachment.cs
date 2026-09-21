using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using FluentValidation;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Attachments;

public sealed record DownloadAttachmentRequest(int AttachmentId);
public sealed record DownloadAttachmentResponse(string FilePath, string ContentType, string FileName);

public interface IDownloadAttachmentHandler : IHandler
{
    Task<Result<DownloadAttachmentResponse>> HandleAsync(int attachmentId, CancellationToken cancellationToken = default);
    Task<Result<DownloadAttachmentResponse>> HandleAsync(DownloadAttachmentRequest request, CancellationToken cancellationToken = default);
}

internal sealed class DownloadAttachmentHandler(
    TicketingDbContext db,
    IConfiguration config) : IDownloadAttachmentHandler
{
    public Task<Result<DownloadAttachmentResponse>> HandleAsync(int attachmentId, CancellationToken ct = default)
        => HandleAsync(new DownloadAttachmentRequest(attachmentId), ct);

    public async Task<Result<DownloadAttachmentResponse>> HandleAsync(DownloadAttachmentRequest request, CancellationToken ct = default)
    {
        var attachmentId = request.AttachmentId;
        string storagePath = config["FileStorage:Path"] ?? "";

        if (string.IsNullOrWhiteSpace(storagePath))
            return Error.Validation("Attachment.StoragePathMissing", "No storage path provided!");

        var attachment = await db.Attachments.FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
        if (attachment == null)
            return Error.NotFound("Attachment.NotFound", $"Attachment with id '{attachmentId}' not found.");

        storagePath += @$"{attachment.Type}";
        var filePath = Path.Combine(storagePath, $"{attachment.Id}{Path.GetExtension(attachment.FileName)}");

        if (!File.Exists(filePath))
            return Error.NotFound("Attachment.FileNotFound", $"File for attachment '{attachmentId}' not found on server.");

        var contentType = GetContentType(filePath);
        return new DownloadAttachmentResponse(filePath, contentType, Path.GetFileName(filePath));
    }

    private static string GetContentType(string path)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
            contentType = "application/octet-stream";
        return contentType;
    }
}

public class DownloadAttachmentRequestValidator : AbstractValidator<DownloadAttachmentRequest>
{
    public DownloadAttachmentRequestValidator()
    {
        RuleFor(x => x.AttachmentId).GreaterThan(0).WithMessage("AttachmentId must be greater than 0");
    }
}
