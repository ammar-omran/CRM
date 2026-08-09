namespace Modules.Ticketing.Domain.Entities;

public class Category
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public bool IsVisible { get; set; }
	public int Sort { get; set; }
	public ICollection<TicketTitle> TicketTitles { get; set; } = [];
}
