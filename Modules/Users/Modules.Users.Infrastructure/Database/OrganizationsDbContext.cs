using CRM.SharedKernel.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.OrganizationAggregate;
using Modules.Users.Domain.UserAggregate;

namespace Modules.Users.Infrastructure.Database;

public class OrganizationsDbContext : DbContext, IApplicationDbContext
{
	public OrganizationsDbContext(DbContextOptions<OrganizationsDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.HasDefaultSchema(DbConsts.OrgSchema);

		// Only apply configurations that exist in the OrganizationAggregate namespace/folder
		modelBuilder.ApplyConfigurationsFromAssembly(
				typeof(OrganizationsDbContext).Assembly,
				type => type.Namespace != null && type.Namespace.Contains("OrganizationAggregate"));
	}

	public DbSet<Agent> Agents { get; set; }
	public DbSet<Organization> Organizations { get; set; }
	public DbSet<OrganizationAgent> OrganizationAgents { get; set; }
	public DbSet<Customer> Customers { get; set; }
}
