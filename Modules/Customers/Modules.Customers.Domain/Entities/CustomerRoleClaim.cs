using Microsoft.AspNetCore.Identity;

namespace Modules.Customers.Domain.Entities;

public class CustomerRoleClaim : IdentityRoleClaim<string>
{
    public CustomerRole Role { get; set; } = null!;
}
