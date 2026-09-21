namespace Modules.Ticketing.Domain.Policies;

/// <summary>
/// Port of TicketManagement SharedKernel.JWT.TicketPermissions.TicketAttachments (5.x)
/// to the modular CRM policy model (crm.ticketing:attachment:*).
/// </summary>
public class AttachmentPolicyConstants
{
    public const string ViewPolicy = "crm.ticketing:attachment:view";
    public const string DownloadPolicy = "crm.ticketing:attachment:download";
    public const string DeletePolicy = "crm.ticketing:attachment:delete";
    public const string UploadPolicy = "crm.ticketing:attachment:upload";
}
