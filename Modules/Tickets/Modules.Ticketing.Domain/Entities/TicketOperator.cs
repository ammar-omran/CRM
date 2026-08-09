namespace Modules.Ticketing.Domain.Entities;

public class TicketOperator
{
	public int Id { get; set; }
	public int TicketId { get; set; }
	public int OperatorId { get; set; }
	public string Role { get; set; } = string.Empty;
	public Ticket Ticket { get; set; } = default!;
	public Operator Operator { get; set; } = default!;
}
