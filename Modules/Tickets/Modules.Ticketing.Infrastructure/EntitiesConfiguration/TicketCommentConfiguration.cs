using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Ticketing.Domain.Entities;

namespace Modules.Ticketing.Infrastructure.EntitiesConfiguration;

public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
	public void Configure(EntityTypeBuilder<TicketComment> builder)
	{
		builder.HasKey(tc => tc.Id);

		builder.Property(tc => tc.TicketId).IsRequired();
		builder.Property(tc => tc.Content).IsRequired().HasMaxLength(1000);
		builder.Property(tc => tc.Commenter).IsRequired();
		builder.Property(tc => tc.CreatedDate).HasDefaultValueSql("GETDATE()");

		builder.HasOne(tc => tc.Ticket)
			.WithMany(t => t.TicketComments)
			.HasForeignKey(tc => tc.TicketId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
