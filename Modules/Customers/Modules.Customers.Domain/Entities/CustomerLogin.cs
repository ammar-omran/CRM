using Microsoft.AspNetCore.Identity;

namespace Modules.Customers.Domain.Entities;

public class CustomerLogin : IdentityUserLogin<string>
{
    public Customer Customer { get; set; } = null!;
}
