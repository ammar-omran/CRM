using Modules.Users.Domain.UserAggregate;

namespace Modules.Users.Domain.OrganizationAggregate;

public class Agent
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = default!;
    public ICollection<OrganizationAgent> AgentOrganizations { get; set; } = [];
}
