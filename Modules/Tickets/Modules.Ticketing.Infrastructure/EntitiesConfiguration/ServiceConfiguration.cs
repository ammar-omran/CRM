using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.EntitiesConfiguration
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name).HasMaxLength(100).IsRequired();


            builder.HasData(
                new Service { Id = 1, Name = "Gas", IsVisible = true, Sort = 1 },
                new Service { Id = 2, Name = "Water", IsVisible = true, Sort = 2 },
                new Service { Id = 3, Name = "Electric", IsVisible = true, Sort = 3 }
            );
        }
    }
}
