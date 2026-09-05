using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Ticketing.Domain.Entities;

namespace Modules.Ticketing.Infrastructure.EntitiesConfiguration;

public class OperatorConfiguration : IEntityTypeConfiguration<Operator>
{
	public void Configure(EntityTypeBuilder<Operator> builder)
	{
		builder.HasKey(o => o.Id);
		builder.Property(o => o.RefId).IsRequired().HasMaxLength(128);
		builder.HasIndex(o => o.RefId).IsUnique();
		builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
		builder.Property(o => o.Email).IsRequired().HasMaxLength(320);
	}
}
