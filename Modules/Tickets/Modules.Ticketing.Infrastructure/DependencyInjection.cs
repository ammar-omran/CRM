using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CRM.SharedKernel.Infrastructure.Database;
using CRM.SharedKernel.Infrastructure.Policies;
using Modules.Ticketing.Infrastructure.Database;
using Modules.Ticketing.Infrastructure.Policies;
using Modules.Ticketing.Domain.Modules;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
	public static IServiceCollection AddTicketingInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddModuleManifest<TicketingModuleManifest>();
		services.AddCoreInfrastructure(configuration, "ticketing");
		services.AddDatabase(configuration);

		services.AddSingleton<IPolicyFactory, TicketingPolicyFactory>();
		services.AddScoped<IModuleDatabaseMigrator, TicketingDatabaseMigrator>();

		return services;
	}

	private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("TicketingDb");

		services.AddDbContext<TicketingDbContext>((provider, options) =>
		{
			var interceptor = provider.GetRequiredService<AuditableInterceptor>();

			options
				.UseSqlServer(connectionString, sqlOptions =>
				{
					sqlOptions.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.Schema);
				})
				.AddInterceptors(interceptor)
				.UseSnakeCaseNamingConvention();
		});

		services.AddSingleton<AuditableInterceptor>();
	}
}
