using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organizations.Domain.Entities;

namespace Modules.Organizations.Infrastructure.Database.Configurations
{
    public class UserCommentConfiguration : IEntityTypeConfiguration<UserComment>
    {
        public void Configure(EntityTypeBuilder<UserComment> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.TicketId).IsRequired();
            builder.Property(c => c.Content).IsRequired().HasMaxLength(1000);
            builder.Property(c => c.CreatedBy).IsRequired(); // Required field
            builder.Property(c => c.CreatedByName).IsRequired().HasMaxLength(150); // Required field
            builder.Property(c => c.CreatedDate).HasDefaultValueSql("GETDATE()");

            // Note: In a real implementation, you would add a foreign key relationship to the Tickets table
            // For now, we'll just configure the TicketId as a required field
        }
    }
}
