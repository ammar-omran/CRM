namespace Modules.Ticketing.Domain.Policies;

public static class TicketPolicyConstants
{
    public const string ViewPolicy = "ticketing:view";
    public const string CreatePolicy = "ticketing:create";
    public const string UpdatePolicy = "ticketing:update";
    public const string DeletePolicy = "ticketing:delete";
    public const string AssignPolicy = "ticketing:assign";
    public const string ReplyPolicy = "ticketing:reply";
}
