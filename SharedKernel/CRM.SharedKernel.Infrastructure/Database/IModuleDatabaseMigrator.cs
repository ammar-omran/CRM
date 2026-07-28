using Microsoft.Extensions.DependencyInjection;

namespace CRM.SharedKernel.Infrastructure.Database;

public interface IModuleDatabaseMigrator
{
    Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default);
}
