using System.ComponentModel.DataAnnotations;

namespace Modules.Ticketing.Domain.Entities;

public class Attachment
{
	public int Id { get; set; }

	[MaxLength(200)]
	public string FileType { get; set; } = string.Empty;

	[MaxLength(200)]
	public string FileName { get; set; } = string.Empty;

	public AttachmentTypeEnum Type { get; set; }
	public int? ReferenceId { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
