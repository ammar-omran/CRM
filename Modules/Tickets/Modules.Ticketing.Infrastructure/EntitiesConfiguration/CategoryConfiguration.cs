using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.OrganizationAggregate.

namespace TicketManagement.Infrastructure.EntitiesConfiguration
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name).HasMaxLength(100).IsRequired();

            builder.HasData(
                new Category { Id = 1, Name = "Recharge Operations", IsVisible = true, Sort = 1 },
                new Category { Id = 2, Name = "Meter & Customer configuration", IsVisible = true, Sort = 2 },
                new Category { Id = 3, Name = "Meter Operations", IsVisible = true, Sort = 3 },
                new Category { Id = 4, Name = "Payment & Settlement Operations", IsVisible = true, Sort = 4 },
                new Category { Id = 5, Name = "System Configuration Errors", IsVisible = true, Sort = 5 },
                new Category { Id = 6, Name = "Reports & Data Issues", IsVisible = true, Sort = 6 },
                new Category { Id = 7, Name = "System Performance & Unexpected Errors", IsVisible = true, Sort = 7 }
            );

            builder.HasMany(c => c.TicketTitles)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
