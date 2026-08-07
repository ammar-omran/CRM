namespace Modules.Users.Domain.OrganizationAggregate;

public class Organization
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
	public ICollection<Customer> OrganizationCustomers { get; set; } = [];
	public ICollection<OrganizationAgent> OrganizationAgents { get; set; } = [];
}
