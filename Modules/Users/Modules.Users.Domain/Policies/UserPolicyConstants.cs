namespace Modules.Users.Domain.Policies;

public class UserPolicyConstants
{
	public const string ReadPolicy = "crm.users:user:read";
	public const string CreatePolicy = "crm.users:user:create";
	public const string UpdatePolicy = "crm.users:user:update";
	public const string DeletePolicy = "crm.users:user:delete";

	public const string AgentReadPolicy = "crm.users:agent:read";
	public const string AgentCreatePolicy = "crm.users:agent:create";
	public const string AgentUpdatePolicy = "crm.users:agent:update";
	public const string AgentDeletePolicy = "crm.users:agent:delete";
}
