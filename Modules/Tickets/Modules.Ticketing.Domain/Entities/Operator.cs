
namespace Modules.Ticketing.Domain.Entities;

public class Operator
{
	public int Id { get; set; }
	public int RefId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;

	public ICollection<TicketOperator> TicketOperators { get; set; } = [];
}
