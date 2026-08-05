using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CRM.SharedKernel.Infrastructure.Database;
using CRM.SharedKernel.Infrastructure.Policies;
using Modules.Customers.Infrastructure.Database;
using Modules.Customers.Infrastructure.Policies;
using Modules.Customers.Domain.Modules;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
	public static IServiceCollection AddCustomersInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddModuleManifest<CustomersModuleManifest>();
		services.AddCoreInfrastructure(configuration, "customers");
		services.AddEmailSender(configuration);
		services.AddDatabase(configuration);

		services.AddSingleton<IPolicyFactory, CustomersPolicyFactory>();
		services.AddScoped<IModuleDatabaseMigrator, CustomersDatabaseMigrator>();

		return services;
	}

	private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("CustomerManagementDb");

		services.AddDbContext<CustomersDbContext>((provider, options) =>
		{
			var interceptor = provider.GetRequiredService<AuditableInterceptor>();

			options
				.UseSqlServer(connectionString, sqlOptions =>
				{
					sqlOptions.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.Schema);
				})
				.AddInterceptors(interceptor);
		});

		services.AddSingleton<AuditableInterceptor>();
	}
}
