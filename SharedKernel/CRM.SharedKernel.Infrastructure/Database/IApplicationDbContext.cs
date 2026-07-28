using Microsoft.EntityFrameworkCore;

namespace CRM.SharedKernel.Infrastructure.Database;

public interface IApplicationDbContext
{
    DbSet<T> Set<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
