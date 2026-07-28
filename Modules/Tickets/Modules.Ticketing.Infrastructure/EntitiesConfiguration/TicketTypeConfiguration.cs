using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.EntitiesConfiguration
{
    public class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
    {
        public void Configure(EntityTypeBuilder<TicketType> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name).HasMaxLength(100).IsRequired();

            builder.HasData(
                new TicketType { Id = 1, Name = "Complaint", IsVisible = true, Sort = 1 },
                new TicketType { Id = 2, Name = "Inquiry", IsVisible = true, Sort = 2 }
            );
        }
    }
}
