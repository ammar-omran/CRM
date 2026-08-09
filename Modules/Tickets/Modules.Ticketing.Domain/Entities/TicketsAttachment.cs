namespace Modules.Ticketing.Domain.Entities;

public class TicketsAttachment
{
	public int Id { get; set; }
	public int TicketId { get; set; }
	public string FileType { get; set; } = string.Empty;
	public string FileName { get; set; } = string.Empty;
	public string FilePath { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public int CreatedBy { get; set; }
	public string CreatedByName { get; set; } = string.Empty;
	public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
	public Ticket Ticket { get; set; } = default!;
}
