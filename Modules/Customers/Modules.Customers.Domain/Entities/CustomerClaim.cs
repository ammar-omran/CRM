using Microsoft.AspNetCore.Identity;

namespace Modules.Customers.Domain.Entities;

public class CustomerClaim : IdentityUserClaim<string>
{
    public Customer Customer { get; set; } = null!;
}
