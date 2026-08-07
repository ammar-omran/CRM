using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.OrganizationAggregate.

namespace TicketManagement.Infrastructure.EntitiesConfiguration
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.HasKey(a => a.Id);
            
            builder.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(a => a.FileType)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(a => a.Type)
                .IsRequired();
                
            builder.Property(a => a.ReferenceId)
                .IsRequired(false);
                
            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
