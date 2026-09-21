using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Attachments;

public sealed record GetAttachmentsByReferenceRequest(int ReferenceId);
public sealed record AttachmentResponse(int FileId, string FileName, string FileType, string AttachmentFor, int? ReferenceId);

public interface IGetAttachmentsByReferenceHandler : IHandler
{
    Task<Result<IReadOnlyList<AttachmentResponse>>> HandleAsync(int referenceId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AttachmentResponse>>> HandleAsync(GetAttachmentsByReferenceRequest request, CancellationToken cancellationToken = default);
}

internal sealed class GetAttachmentsByReferenceHandler(
    TicketingDbContext db,
    ILogger<GetAttachmentsByReferenceHandler> logger) : IGetAttachmentsByReferenceHandler
{
    public Task<Result<IReadOnlyList<AttachmentResponse>>> HandleAsync(int referenceId, CancellationToken ct = default)
        => HandleAsync(new GetAttachmentsByReferenceRequest(referenceId), ct);

    public async Task<Result<IReadOnlyList<AttachmentResponse>>> HandleAsync(GetAttachmentsByReferenceRequest request, CancellationToken ct = default)
    {
        var referenceId = request.ReferenceId;
        List<Modules.Ticketing.Domain.Entities.Attachment> attachments;
        try
        {
            attachments = await db.Attachments
                .AsNoTracking()
                .Where(a => a.ReferenceId == referenceId)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching attachments for reference {ReferenceId}", referenceId);
            return Error.Failure("Attachment.DbError", ex.Message);
        }

        if (attachments == null || attachments.Count == 0)
            return Error.NotFound("Attachment.NotFound", $"No attachments found for reference ID '{referenceId}'.");

        var dtos = attachments.Select(a => new AttachmentResponse(
            a.Id,
            a.FileName,
            a.FileType,
            a.Type.ToString(),
            a.ReferenceId)).ToList();

        return dtos;
    }
}

public class GetAttachmentsByReferenceRequestValidator : AbstractValidator<GetAttachmentsByReferenceRequest>
{
    public GetAttachmentsByReferenceRequestValidator()
    {
        RuleFor(x => x.ReferenceId).GreaterThan(0).WithMessage("ReferenceId must be greater than 0");
    }
}
