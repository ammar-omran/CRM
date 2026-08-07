namespace Modules.Users.Domain.OrganizationAggregate;

public class OrganizationAgent
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; }

    public int AgentId { get; set; }
    public Agent Agent { get; set; }

    public int AgentRoleId { get; set; }
    public AgentRole AgentRole { get; set; }
}
