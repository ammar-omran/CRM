using Microsoft.AspNetCore.Identity;

namespace Modules.Customers.Domain.Entities;

public class CustomerUserRole : IdentityUserRole<string>
{
    public Customer Customer { get; set; } = null!;
    public CustomerRole Role { get; set; } = null!;
}
