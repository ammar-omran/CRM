using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customers.Domain.Entities;

namespace Modules.Customers.Infrastructure.Database.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Email is already part of IdentityUser, but ensure max length
        builder.Property(x => x.Email)
            .HasMaxLength(256);

        builder.Property(x => x.NormalizedEmail)
            .HasMaxLength(256);

        builder.Property(x => x.UserName)
            .HasMaxLength(256);

        builder.Property(x => x.NormalizedUserName)
            .HasMaxLength(256);

        builder.Property(x => x.OTP)
            .HasMaxLength(10);

        builder.Property(x => x.HashedEmail)
            .HasMaxLength(200);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Each Customer can have many CustomerClaims
        builder.HasMany(e => e.Claims)
            .WithOne(e => e.Customer)
            .HasForeignKey(uc => uc.UserId)
            .IsRequired();

        // Each Customer can have many CustomerLogins
        builder.HasMany(e => e.UserLogins)
            .WithOne(e => e.Customer)
            .HasForeignKey(ul => ul.UserId)
            .IsRequired();

        // Each Customer can have many CustomerTokens
        builder.HasMany(e => e.UserTokens)
            .WithOne(e => e.Customer)
            .HasForeignKey(ut => ut.UserId)
            .IsRequired();

        // Each Customer can have many roles
        builder.HasMany(e => e.UserRoles)
            .WithOne(e => e.Customer)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        // Map owned PhoneNumber value object (hides base PhoneNumber string)
        builder.OwnsOne(x => x.PhoneNumber, pn =>
        {
            pn.Property(p => p.Number)
                .HasColumnName("PhoneNumber_Number")
                .HasMaxLength(20)
                .IsRequired();

            pn.Property(p => p.CountryCode)
                .HasColumnName("PhoneNumber_CountryCode")
                .HasMaxLength(5);

            pn.HasIndex(p => p.Number).HasDatabaseName("IX_Customers_PhoneNumber_Number");
        });

        // Ignore base IdentityUser phone string to avoid duplicate column - use owned PhoneNumber instead
        builder.Ignore(x => x.PhoneNumberConfirmed);
        // Note: base PhoneNumber (string) is hidden by new PhoneNumber (PhoneNumber type), so no extra ignore needed
    }
}
