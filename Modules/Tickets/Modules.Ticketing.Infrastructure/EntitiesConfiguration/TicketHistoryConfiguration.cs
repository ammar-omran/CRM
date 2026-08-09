using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Ticketing.Domain.Entities;

namespace Modules.Ticketing.Infrastructure.EntitiesConfiguration
{
    public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
    {
        public void Configure(EntityTypeBuilder<TicketHistory> builder)
        {
            builder.HasKey(u => u.Id);


            builder.Property(th => th.TicketId).IsRequired();

            builder.Property(th => th.CreatedDate)
            .HasDefaultValueSql("GETDATE()");
        }
    }
}
