using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CRM.SharedKernel.Infrastructure.Database;
using CRM.SharedKernel.Infrastructure.Policies;
using Modules.Users.Domain.Authentication;
using Modules.Users.Domain.Users;
using Modules.Users.Infrastructure.Authorization;
using Modules.Users.Infrastructure.Database;
using Modules.Users.Infrastructure.Policies;
using Modules.Users.Domain.Modules;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
	public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDatabase(configuration);
		services.AddModuleManifest<UsersModuleManifest>();
		services.AddCoreInfrastructure(configuration, "users");

		services.AddScoped<IClientAuthorizationService, ClientAuthorizationService>();

		services.AddSingleton<IPolicyFactory, UsersPolicyFactory>();

		return services;
	}

	private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("UsersDb");

		services.AddDbContext<UsersDbContext>((provider, options) =>
		{
			var interceptor = provider.GetRequiredService<AuditableInterceptor>();

			options
				.UseSqlServer(connectionString, mssqlOptions =>
				{
					mssqlOptions.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.Schema);
				})
				.AddInterceptors(interceptor)
				.UseSnakeCaseNamingConvention();
		});

		services.AddScoped<IModuleDatabaseMigrator, UsersDatabaseMigrator>();

		services.AddSingleton<AuditableInterceptor>();

		services
			.AddIdentityCore<User>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireUppercase = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequiredLength = 8;
			})
			.AddRoles<Role>()
			.AddEntityFrameworkStores<UsersDbContext>()
			.AddSignInManager()
			.AddDefaultTokenProviders();
	}
}
