using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CRM.SharedKernel.Domain;

namespace CRM.SharedKernel.Infrastructure.Database;

public class AuditableInterceptor : SaveChangesInterceptor
{
	public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default)
	{
		var context = eventData.Context!;
		var entries = context.ChangeTracker.Entries<IAuditableEntity>();

		foreach (var entry in entries)
		{
			if (entry.State == EntityState.Added)
			{
				entry.Entity.CreatedAt = DateTime.Now;
			}
			else if (entry.State == EntityState.Modified)
			{
				entry.Entity.UpdatedAt = DateTime.Now;
			}
		}

		return await base.SavingChangesAsync(eventData, result, cancellationToken);
	}
}
