namespace Modules.Organizations.Domain.Entities;

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<OrganizationCustomer> OrganizationCustomers { get; set; } = [];
    public ICollection<OrganizationAgent> OrganizationAgents { get; set; } = [];
}
