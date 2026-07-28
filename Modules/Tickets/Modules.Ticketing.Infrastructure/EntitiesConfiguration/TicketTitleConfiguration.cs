using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.EntitiesConfiguration
{
    public class TicketTitleConfiguration : IEntityTypeConfiguration<TicketTitle>
    {
        public void Configure(EntityTypeBuilder<TicketTitle> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
            builder.Property(t => t.CategoryId).IsRequired();
            builder.Property(t => t.CreatedDate).IsRequired().HasDefaultValueSql("GETDATE()");

            builder.HasData(
                new TicketTitle { Id = 1, Name = "Failed Recharge", CategoryId = 1, DefaultSeverityId = 4, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 2, Name = "Wrong Balance After Recharge", CategoryId = 1, DefaultSeverityId = 4, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 3, Name = "Wrong Payment Type", CategoryId = 1, DefaultSeverityId = 3, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 4, Name = "Add/Edit (Meter or Customer data )", CategoryId = 2, DefaultSeverityId = 3, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 5, Name = "System Card issue", CategoryId = 2, DefaultSeverityId = 2, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 6, Name = "Customer Card Issue", CategoryId = 3, DefaultSeverityId = 3, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 7, Name = "Link Meter to Customer Failed", CategoryId = 3, DefaultSeverityId = 3, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 8, Name = "Read retrival card issue", CategoryId = 3, DefaultSeverityId = 2, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 9, Name = "Replace Card Issue", CategoryId = 3, DefaultSeverityId = 3, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 10, Name = "Replace Meter Issue", CategoryId = 3, DefaultSeverityId = 2, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 11, Name = "Adjustment Issue", CategoryId = 4, DefaultSeverityId = 3, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 12, Name = "Payment Order Issue", CategoryId = 4, DefaultSeverityId = 3, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 13, Name = "Debit Settlement Issue", CategoryId = 4, DefaultSeverityId = 3, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 14, Name = "Pay Adjustment Issue", CategoryId = 4, DefaultSeverityId = 3, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 15, Name = "Add/Edit(Users, Privileges, Cashier Holiday, Friendly Time, General Settings,Fees, Tariff, Customer Complaints, Activity, Adjustment Type, Banks, Organization, Replace Meter Reasons, Services)", CategoryId = 5, DefaultSeverityId = 2, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 16, Name = "Depends on Report Type", CategoryId = 6, DefaultSeverityId = 1, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new TicketTitle { Id = 17, Name = "System Crash / Slow / Unexpected Error", CategoryId = 7, DefaultSeverityId = 4, CreatedDate = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) }
            );
        }
    }
}
