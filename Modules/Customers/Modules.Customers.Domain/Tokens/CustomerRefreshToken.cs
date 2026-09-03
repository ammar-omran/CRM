using CRM.SharedKernel.Domain;
using Modules.Customers.Domain.Entities;

namespace Modules.Customers.Domain.Tokens;

public class CustomerRefreshToken : IAuditableEntity
{
    public string Token { get; set; } = null!;
    public string JwtId { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }
    public bool Invalidated { get; set; }
    public string CustomerId { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
