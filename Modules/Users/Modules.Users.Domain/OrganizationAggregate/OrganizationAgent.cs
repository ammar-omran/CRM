using Modules.Users.Domain.UserAggregate;

namespace Modules.Users.Domain.OrganizationAggregate;

public class OrganizationAgent
{
	public int Id { get; set; }
	public int OrganizationId { get; set; }
	public Organization Organization { get; set; } = default!;

	public int AgentId { get; set; }
	public Agent Agent { get; set; } = default!;

	public string AgentRoleId { get; set; } = string.Empty;
	public Role AgentRole { get; set; } = default!;
}
