using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Attachments;

public sealed record UpdateAttachmentReferenceRequest(int AttachmentId, int ReferenceId);

public interface IUpdateAttachmentReferenceHandler : IHandler
{
    Task<Result<Success>> HandleAsync(int attachmentId, int referenceId, CancellationToken cancellationToken = default);
    Task<Result<Success>> HandleAsync(UpdateAttachmentReferenceRequest request, CancellationToken cancellationToken = default);
}

internal sealed class UpdateAttachmentReferenceHandler(
    TicketingDbContext db,
    ILogger<UpdateAttachmentReferenceHandler> logger) : IUpdateAttachmentReferenceHandler
{
    public Task<Result<Success>> HandleAsync(int attachmentId, int referenceId, CancellationToken ct = default)
        => HandleAsync(new UpdateAttachmentReferenceRequest(attachmentId, referenceId), ct);

    public async Task<Result<Success>> HandleAsync(UpdateAttachmentReferenceRequest request, CancellationToken ct = default)
    {
        var attachment = await db.Attachments.FirstOrDefaultAsync(a => a.Id == request.AttachmentId, ct);
        if (attachment is null)
            return Error.NotFound("Attachment.NotFound", $"Attachment with id '{request.AttachmentId}' Not Found!");

        attachment.ReferenceId = request.ReferenceId;

        try
        {
            db.Attachments.Update(attachment);
            var saved = await db.SaveChangesAsync(ct) > 0;
            if (!saved)
                return Error.Failure("Attachment.DbError", $"Failed to update attachment reference id for attachment '{attachment.Id}'.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating attachment {Id} reference", request.AttachmentId);
            return Error.Failure("Attachment.DbError", ex.Message);
        }

        logger.LogInformation("Attachment '{Id}' reference id updated successfully to {ReferenceId}", attachment.Id, request.ReferenceId);
        return Result.Success;
    }
}

public class UpdateAttachmentReferenceRequestValidator : AbstractValidator<UpdateAttachmentReferenceRequest>
{
    public UpdateAttachmentReferenceRequestValidator()
    {
        RuleFor(x => x.AttachmentId).GreaterThan(0).WithMessage("AttachmentId must be greater than 0");
        RuleFor(x => x.ReferenceId).GreaterThan(0).WithMessage("ReferenceId must be greater than 0");
    }
}
