using Microsoft.EntityFrameworkCore;

namespace CRM.Base.Modules.Persistence;

/// <summary>
/// SQLite-backed persistence for registered modules.
/// Uses EF Core with a single RegisteredModules table.
/// </summary>
public sealed class ModulePersistence : IModulePersistence
{
	private readonly IDbContextFactory<PlatformDbContext> _factory;

	public ModulePersistence(IDbContextFactory<PlatformDbContext> factory)
	{
		_factory = factory;
	}

	/// <inheritdoc />
	public async Task SaveAsync(RegisteredModuleEntity entity)
	{
		await using var db = await _factory.CreateDbContextAsync();
		db.RegisteredModules.Add(entity);
		await db.SaveChangesAsync();
	}

	/// <inheritdoc />
	public async Task UpdateAsync(RegisteredModuleEntity entity)
	{
		await using var db = await _factory.CreateDbContextAsync();
		db.RegisteredModules.Update(entity);
		await db.SaveChangesAsync();
	}

	/// <inheritdoc />
	public async Task<bool> RemoveAsync(string moduleId)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var entity = await db.RegisteredModules
			.FirstOrDefaultAsync(m => m.ModuleId == moduleId);

		if (entity is null)
		{
			return false;
		}

		db.RegisteredModules.Remove(entity);
		await db.SaveChangesAsync();
		return true;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<RegisteredModuleEntity>> LoadAllAsync()
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.RegisteredModules
			.AsNoTracking()
			.ToListAsync();
	}

	/// <inheritdoc />
	public async Task<bool> ExistsAsync(string moduleId)
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.RegisteredModules.AnyAsync(m => m.ModuleId == moduleId);
	}
}
