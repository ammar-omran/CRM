using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CRM.SharedKernel.Infrastructure.Database;
using CRM.SharedKernel.Infrastructure.Policies;
using Modules.Users.Domain.Authentication;
using Modules.Users.Domain.UserAggregate;
using Modules.Users.Infrastructure.Authorization;
using Modules.Users.Infrastructure.Database;
using Modules.Users.Infrastructure.Policies;
using Modules.Users.Domain.Modules;
using Modules.Users.Domain.Repositories;
using Modules.Users.Infrastructure.Repositories;
using CRM.SharedKernel.Domain.Authorization;
using CRM.SharedKernel.Domain.Interfaces;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
	public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDatabase(configuration);
		services.AddModuleManifest<UsersModuleManifest>();
		services.AddCoreInfrastructure(configuration);

		services.AddScoped<IClientAuthorizationService, ClientAuthorizationService>();

		services.AddScoped<IPasswordResetTokenGenerator, PasswordResetTokenGenerator>();

		services.AddSingleton<IPolicyFactory, UsersPolicyFactory>();

		services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
		services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
		services.AddScoped<ICustomerRepository, CustomerRepository>();

		// Portal-module role & permission store (owns the module's RBAC data).
		services.AddScoped<IRolePermissionStore, UsersRolePermissionStore>();
		services.AddHttpClient("Modules.CustomersApi");

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
					mssqlOptions.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.UsersSchema);
				})
				.AddInterceptors(interceptor);
		});

		services.AddDbContext<OrganizationsDbContext>((provider, options) =>
		{
			var interceptor = provider.GetRequiredService<AuditableInterceptor>();

			options
				.UseSqlServer(connectionString, mssqlOptions =>
				{
					mssqlOptions.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.OrgSchema);
				})
				.AddInterceptors(interceptor);
		});

		services.AddScoped<IApplicationDbContext>(sp =>
			sp.GetRequiredService<OrganizationsDbContext>());

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
