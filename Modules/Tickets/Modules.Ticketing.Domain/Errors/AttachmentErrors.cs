using CRM.SharedKernel.Domain.Results;

namespace Modules.Ticketing.Domain.Errors;

public static class AttachmentErrors
{
    public static Error NotFound(int id) =>
        Error.NotFound("Attachment.NotFound", $"Attachment with ID {id} was not found.");

    public static Error NotFoundByReference(int referenceId) =>
        Error.NotFound("Attachment.NotFound", $"No attachments found for reference {referenceId}.");

    public static Error InvalidType =>
        Error.Validation("Attachment.InvalidType", "Invalid attachment type.");

    public static Error FileRequired =>
        Error.Validation("Attachment.FileRequired", "File is required.");

    public static Error FileTooLarge(string max) =>
        Error.Validation("Attachment.FileTooLarge", $"File exceeds maximum allowed size of {max}.");

    public static Error UnsupportedExtension =>
        Error.Validation("Attachment.UnsupportedExtension", "Unsupported file type.");

    public static Error LimitExceeded(int limit) =>
        Error.Validation("Attachment.LimitExceeded", $"Attachment limit of {limit} exceeded for this reference.");

    public static Error StorageFailure =>
        Error.Failure("Attachment.StorageFailure", "Failed to store attachment file.");

    public static Error FileNotFoundOnDisk =>
        Error.NotFound("Attachment.FileNotFoundOnDisk", "Attachment file not found on disk.");
}
