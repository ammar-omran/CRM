using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Ticketing.Domain.Entities;

namespace Modules.Ticketing.Infrastructure.EntitiesConfiguration;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
	public void Configure(EntityTypeBuilder<Ticket> builder)
	{
		builder.HasKey(t => t.Id);

		builder.Property(t => t.OtherTitle).HasMaxLength(100);
		builder.Property(t => t.Description).IsRequired().HasMaxLength(1000);
		builder.Property(t => t.CategoryId).IsRequired();
		builder.Property(t => t.TypeId).IsRequired();
		builder.Property(t => t.Status).HasConversion<short>();
		builder.Property(t => t.CreatedAt).HasDefaultValueSql("GETDATE()");
		builder.Property(t => t.UpdatedAt);

		builder.HasOne(t => t.Category)
			.WithMany()
			.HasForeignKey(t => t.CategoryId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(t => t.Type)
			.WithMany()
			.HasForeignKey(t => t.TypeId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(t => t.Severity)
			.WithMany()
			.HasForeignKey(t => t.SeverityId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasOne(t => t.TicketTitle)
			.WithMany()
			.HasForeignKey(t => t.TitleId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(t => t.TicketHistories)
			.WithOne(th => th.Ticket)
			.HasForeignKey(th => th.TicketId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(t => t.TicketOperators)
			.WithOne(to => to.Ticket)
			.HasForeignKey(to => to.TicketId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
