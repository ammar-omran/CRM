using CRM.SharedKernel.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Ticketing.Infrastructure.Database;

public static class DbConsts
{
    public const string Schema = "ticketing";
    public const string MigrationTableName = "__EFMigrationsHistory";
}

public class TicketingDatabaseMigrator : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
