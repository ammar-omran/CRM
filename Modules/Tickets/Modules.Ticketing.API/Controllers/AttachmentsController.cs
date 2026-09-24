using CRM.SharedKernel.Application.API.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Ticketing.Domain.Entities;
using Modules.Ticketing.Domain.Policies;
using Modules.Ticketing.Features.Attachments;

namespace Modules.Ticketing.API.Controllers;

/// <summary>
/// Handles operations related to file attachments.
/// Handler pattern (IHandler + Result + FluentValidation) with EF Core DbContext directly,
/// method bodies ported verbatim from TicketManagement.Application.Services.AttachmentsService.
/// Route prefix /api/attachments matches TicketingModuleManifest.
/// </summary>
[Route("api/attachments")]
[ApiController]
public class AttachmentsController : ControllerBase
{
    private static Dictionary<string, string[]> ToDictionary(FluentValidation.Results.ValidationResult validation)
        => validation.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

    /// <summary>
    /// Retrieves all attachments associated with a specific reference ID.
    /// </summary>
    // [Authorize(Policy = AttachmentPolicyConstants.ViewPolicy)]
    [HttpGet("reference/{referenceId}")]
    public async Task<IActionResult> GetAttachmentsOfReference(
        int referenceId,
        [FromServices] IValidator<GetAttachmentsByReferenceRequest> validator,
        [FromServices] IGetAttachmentsByReferenceHandler handler,
        CancellationToken ct)
    {
        var request = new GetAttachmentsByReferenceRequest(referenceId);
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return BadRequest(ToDictionary(validation));

        var result = await handler.HandleAsync(request, ct);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }

    /// <summary>
    /// Downloads an attachment file by its ID.
    /// </summary>
    // [Authorize(Policy = AttachmentPolicyConstants.DownloadPolicy)]
    [HttpGet("download/{attachmentId}")]
    public async Task<IActionResult> DownloadAttachment(
        int attachmentId,
        [FromServices] IValidator<DownloadAttachmentRequest> validator,
        [FromServices] IDownloadAttachmentHandler handler,
        CancellationToken ct)
    {
        var request = new DownloadAttachmentRequest(attachmentId);
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return BadRequest(ToDictionary(validation));

        var result = await handler.HandleAsync(request, ct);
        if (result.IsError) return result.Errors.ToMVCProblem();
        return PhysicalFile(result.Value!.FilePath, result.Value!.ContentType, result.Value!.FileName);
    }

    // [Authorize(Policy = AttachmentPolicyConstants.DeletePolicy)]
    [HttpDelete("{attachmentId}")]
    public async Task<IActionResult> DeleteAttachment(
        int attachmentId,
        [FromServices] IValidator<DeleteAttachmentRequest> validator,
        [FromServices] IDeleteAttachmentHandler handler,
        CancellationToken ct)
    {
        var request = new DeleteAttachmentRequest(attachmentId);
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return BadRequest(ToDictionary(validation));

        var result = await handler.HandleAsync(request, ct);
        return result.IsError ? result.Errors.ToMVCProblem() : NoContent();
    }

    /// <summary>
    /// Sets or updates the reference ID for an existing attachment.
    /// </summary>
    // [Authorize(Policy = AttachmentPolicyConstants.UploadPolicy)]
    [HttpPatch("{attachmentId}/set-reference/{referenceId}")]
    public async Task<IActionResult> SetAttachmentReference(
        int attachmentId,
        int referenceId,
        [FromServices] IValidator<UpdateAttachmentReferenceRequest> validator,
        [FromServices] IUpdateAttachmentReferenceHandler handler,
        CancellationToken ct)
    {
        var request = new UpdateAttachmentReferenceRequest(attachmentId, referenceId);
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return BadRequest(ToDictionary(validation));

        var result = await handler.HandleAsync(request, ct);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok();
    }

    /// <summary>
    /// Uploads a new attachment. Type is AttachmentTypeEnum (None/Ticket/Comment).
    /// </summary>
    // [Authorize(Policy = AttachmentPolicyConstants.UploadPolicy)]
    [HttpPost("{type}/{referenceId}")]
    public async Task<IActionResult> UploadAttachment(
        AttachmentTypeEnum type,
        int? referenceId,
        IFormFile file,
        [FromServices] IValidator<UploadAttachmentRequest> validator,
        [FromServices] IUploadAttachmentHandler handler,
        CancellationToken ct)
    {
        var request = new UploadAttachmentRequest(type, referenceId, file);
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return BadRequest(ToDictionary(validation));

        var result = await handler.HandleAsync(request, ct);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }
}
