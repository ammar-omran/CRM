using Microsoft.AspNetCore.Identity;

namespace Modules.Customers.Domain.Entities;

public class CustomerRole : IdentityRole
{
    public ICollection<CustomerRoleClaim> RoleClaims { get; set; } = null!;
}
