using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CRM.SharedKernel.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Database;

public class UsersDatabaseMigrator : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        // Users schema owns the shared roles table and must migrate first,
        // since OrganizationAgents holds an FK referencing users.roles.
        await scope.ServiceProvider
                .GetRequiredService<UsersDbContext>()
                .Database.MigrateAsync(cancellationToken);

        await scope.ServiceProvider
                .GetRequiredService<OrganizationsDbContext>()
                .Database.MigrateAsync(cancellationToken);
    }
}
