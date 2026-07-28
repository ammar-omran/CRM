using CRM.SharedKernel.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Modules.Organizations.Domain.Entities;

namespace Modules.Organizations.Infrastructure.Database;

public class OrganizationsDbContext : DbContext, IApplicationDbContext
{
    public OrganizationsDbContext(DbContextOptions<OrganizationsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationsDbContext).Assembly);
    }

    public DbSet<Agent> Agents { get; set; }
    public DbSet<AgentRole> AgentRoles { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationAgent> OrganizationAgents { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<OrganizationCustomer> OrganizationCustomers { get; set; }
    public DbSet<UserComment> UserComments { get; set; }
}
