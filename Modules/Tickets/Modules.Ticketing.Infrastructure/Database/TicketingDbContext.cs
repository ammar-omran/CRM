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
    public DbSet<Operator> Operators { get; set; } = null!;
    public DbSet<TicketOperator> TicketOperators { get; set; } = null!;
    public DbSet<TicketComment> TicketComments { get; set; } = null!;
    public DbSet<TicketHistory> TicketHistories { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<TicketType> TicketTypes { get; set; } = null!;
    public DbSet<TicketTitle> TicketTitles { get; set; } = null!;
    public DbSet<Severity> Severities { get; set; } = null!;
    public DbSet<TicketsAttachment> TicketsAttachments { get; set; } = null!;
}
