namespace Modules.Ticketing.Domain.Policies;

public class TicketPolicyConstants
{
	public const string ViewAllPolicy = "crm.ticketing:ticket:view-all";
	public const string ViewMinePolicy = "crm.ticketing:ticket:view-mine";
	public const string ViewGroupPolicy = "crm.ticketing:ticket:view-group";
	public const string ViewAnyPolicy = "crm.ticketing:ticket:view-any";
	public const string CreatePolicy = "crm.ticketing:ticket:create";
	public const string AssignPolicy = "crm.ticketing:ticket:assign";
	public const string ReplyPolicy = "crm.ticketing:ticket:reply";
}
