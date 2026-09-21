namespace Modules.Ticketing.Features.Attachments;

public class AttachmentConfigs
{
    public string AttachmentFor { get; set; } = "None";
    public int LimitNumber { get; set; } = 1;
    public int MaxSizeMB { get; set; }
    public string[] AllowedExtensions { get; set; } = [];
}
