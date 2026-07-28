namespace Modules.Organizations.Domain.Entities;

public class Agent
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ICollection<OrganizationAgent> AgentOrganizations { get; set; } = [];
}
