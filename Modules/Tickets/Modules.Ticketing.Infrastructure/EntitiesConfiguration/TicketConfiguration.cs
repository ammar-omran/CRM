using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.EntitiesConfiguration
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(th => th.CategoryId).IsRequired();
            builder.Property(th => th.TypeId).IsRequired();
            builder.Property(th => th.SeverityId);
            builder.HasOne(t => t.Severity)
                   .WithMany()
                   .HasForeignKey(t => t.SeverityId)
                   .OnDelete(DeleteBehavior.SetNull);
            builder.Property(th => th.TitleId);
            builder.HasOne(t => t.TicketTitle)
                   .WithMany()
                   .HasForeignKey(t => t.TitleId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.Property(th => th.Title).HasMaxLength(100);
            builder.Property(th => th.Description).IsRequired().HasMaxLength(1000);
            builder.Property(th => th.Status).IsRequired();
            builder.Property(th => th.CustomerId).IsRequired();
            builder.Property(th => th.CustomerName).IsRequired().HasMaxLength(150);

            builder.HasMany(t => t.TicketHistories)
                   .WithOne(th => th.Ticket)
                   .HasForeignKey(th => th.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.ticketAttachments)
                   .WithOne(th => th.Ticket)
                   .HasForeignKey(th => th.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(th => th.CreatedDate)
            .HasDefaultValueSql("GETDATE()");

            builder.Property(th => th.UpdatedDate)
            .HasDefaultValueSql("GETDATE()");

            builder.Property(t => t.Status)
                .HasConversion<short>();
        }
    }
}
