using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace Modules.Ticketing.Infrastructure.Database.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(4000);

        entity.Property(x => x.CustomerEmail)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.CustomerName)
            .HasMaxLength(150);

        entity.Property(x => x.CreatedByName)
            .HasMaxLength(150);

        entity.Property(x => x.UpdatedByName)
            .HasMaxLength(150);
    }
}
