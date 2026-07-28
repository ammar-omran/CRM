using CRM.SharedKernel.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Customers.Infrastructure.Database;

public static class DbConsts
{
    public const string Schema = "customers";
    public const string MigrationTableName = "__EFMigrationsHistory";
}

public class CustomersDatabaseMigrator : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
