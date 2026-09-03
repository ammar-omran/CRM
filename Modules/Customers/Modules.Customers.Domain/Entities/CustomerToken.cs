using Microsoft.AspNetCore.Identity;

namespace Modules.Customers.Domain.Entities;

public class CustomerToken : IdentityUserToken<string>
{
    public Customer Customer { get; set; } = null!;
}
