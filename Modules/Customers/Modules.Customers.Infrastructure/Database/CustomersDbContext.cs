using CRM.SharedKernel.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Modules.Customers.Domain.OrganizationAggregate.

namespace Modules.Customers.Infrastructure.Database;

public class CustomersDbContext : DbContext, IApplicationDbContext
{
	public CustomersDbContext(DbContextOptions<CustomersDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomersDbContext).Assembly);
	}

	public DbSet<Customer> Customers { get; set; } = null!;
}
