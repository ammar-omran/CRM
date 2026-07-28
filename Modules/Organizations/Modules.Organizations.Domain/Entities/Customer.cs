namespace Modules.Organizations.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int ReferenceId { get; set; } // customer Id in Modules.Customers
    public ICollection<OrganizationCustomer> OrganizationCustomers { get; set; } = [];
}
