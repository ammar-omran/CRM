using CRM.SharedKernel.Infrastructure.Database;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Modules.Customers.Domain.Entities;
using Modules.Customers.Domain.Tokens;

namespace Modules.Customers.Infrastructure.Database;

public class CustomersDbContext : IdentityDbContext<Customer, CustomerRole, string,
	CustomerClaim, CustomerUserRole, CustomerLogin,
	CustomerRoleClaim, CustomerToken>, IApplicationDbContext
{
	public DbSet<CustomerRefreshToken> RefreshTokens { get; set; } = null!;

	public CustomersDbContext(DbContextOptions<CustomersDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.HasDefaultSchema(DbConsts.Schema);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomersDbContext).Assembly);
	}

	// Keep legacy DbSet name for compatibility with existing queries
	public DbSet<Customer> Customers => Set<Customer>();
}
