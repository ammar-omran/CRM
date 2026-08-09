using CRM.SharedKernel.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Modules.Ticketing.Domain.Entities;

namespace Modules.Ticketing.Infrastructure.Database;

public class TicketingDbContext : DbContext, IApplicationDbContext
{
    public TicketingDbContext(DbContextOptions<TicketingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketingDbContext).Assembly);
        modelBuilder.HasDefaultSchema("ticketing");
    }

    public DbSet<Ticket> Tickets { get; set; } = null!;
}
