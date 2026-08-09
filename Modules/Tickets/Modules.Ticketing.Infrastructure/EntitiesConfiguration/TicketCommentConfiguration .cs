using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Ticketing.Domain.Entities;

namespace Modules.Ticketing.Infrastructure.EntitiesConfiguration
{
    public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
    {
        public void Configure(EntityTypeBuilder<TicketComment> builder)
        {
            builder.HasKey(tc => tc.Id);

            builder.Property(c => c.TicketId).IsRequired();
builder.Property(c => c.Description).IsRequired().HasMaxLength(1000);
builder.Property(c => c.CreatedBy);
builder.Property(c => c.CreatedByName).HasMaxLength(150);
builder.Property(c => c.CreatedDate).HasDefaultValueSql("GETDATE()");

builder.HasOne(c => c.Ticket)
       .WithMany()
       .HasForeignKey(c => c.TicketId)
       .OnDelete(DeleteBehavior.Cascade);

            builder.Property(tc => tc.Description)
                   .IsRequired()
                   .HasMaxLength(1000);


            builder.HasOne(tc => tc.Ticket)
       .WithMany(t => t.ticketComments)
       .HasForeignKey(tc => tc.TicketId)
       .OnDelete(DeleteBehavior.Cascade);

            builder.Property(tc => tc.CreatedDate)
                   .HasDefaultValueSql("GETDATE()");
        }
    }
}
