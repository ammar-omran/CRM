using Microsoft.EntityFrameworkCore;

namespace CRM.Base.Modules.Persistence;

/// <summary>
/// EF Core DbContext for the platform's SQLite database.
/// Stores registered modules as JSON blobs — the schema is intentionally minimal.
/// </summary>
public sealed class PlatformDbContext : DbContext
{
	/// <summary>
	/// The set of registered modules.
	/// </summary>
	public DbSet<RegisteredModuleEntity> RegisteredModules => Set<RegisteredModuleEntity>();

	public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options)
	{
	}

	/// <inheritdoc />
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<RegisteredModuleEntity>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.HasIndex(e => e.ModuleId)
				.IsUnique();

			entity.Property(e => e.ModuleId)
				.HasMaxLength(256)
				.IsRequired();

			entity.Property(e => e.DisplayName)
				.HasMaxLength(512)
				.IsRequired();

			entity.Property(e => e.BaseUrl)
				.HasMaxLength(2048)
				.IsRequired();

			entity.Property(e => e.ManifestJson)
				.IsRequired();

			entity.Property(e => e.RegisteredAt)
				.IsRequired();

			entity.Property(e => e.UpdatedAt)
				.IsRequired();
		});
	}
}
