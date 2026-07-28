using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure
{
    public class TicketManagementContext : DbContext
    {
        public TicketManagementContext(DbContextOptions<TicketManagementContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Severity> Severties { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketsAttachment> TicketsAttachments { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketTitle> TicketTitles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketManagementContext).Assembly);
        }
    }
}
