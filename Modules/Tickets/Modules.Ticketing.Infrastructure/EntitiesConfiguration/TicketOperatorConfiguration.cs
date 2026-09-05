using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Ticketing.Domain.Entities;

namespace Modules.Ticketing.Infrastructure.EntitiesConfiguration;

public class TicketOperatorConfiguration : IEntityTypeConfiguration<TicketOperator>
{
	public void Configure(EntityTypeBuilder<TicketOperator> builder)
	{
		builder.HasKey(to => to.Id);
		builder.Property(to => to.Role)
			.IsRequired()
			.HasMaxLength(256);
		builder.HasIndex(to => new { to.TicketId, to.OperatorId }).IsUnique();

		builder.HasOne(to => to.Ticket)
			.WithMany(o => o.TicketOperators)
			.HasForeignKey(to => to.OperatorId)
			.OnDelete(DeleteBehavior.Cascade);
		// Ticket side is configured in TicketConfiguration; only configure Operator side here.
		builder.HasOne(to => to.Operator)
			.WithMany(o => o.TicketOperators)
			.HasForeignKey(to => to.OperatorId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
