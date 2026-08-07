using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.OrganizationAggregate.

namespace TicketManagement.Infrastructure.EntitiesConfiguration
{
    public class TicketsAttachmentConfiguration : IEntityTypeConfiguration<TicketsAttachment>
    {
        public void Configure(EntityTypeBuilder<TicketsAttachment> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(th => th.TicketId).IsRequired();
            builder.Property(th => th.FileType).IsRequired().HasMaxLength(50);
            builder.Property(th => th.FileName).IsRequired().HasMaxLength(100);
            builder.Property(th => th.FilePath).IsRequired().HasMaxLength(200);
            builder.Property(th => th.Description).IsRequired().HasMaxLength(200);

            builder.Property(th => th.CreatedDate)
            .HasDefaultValueSql("GETDATE()");
        }
    }
}
