using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CRM.SharedKernel.Infrastructure.Database;
using CRM.SharedKernel.Infrastructure.Policies;
using Modules.Customers.Domain.Authentication;
using Modules.Customers.Domain.Entities;
using Modules.Customers.Infrastructure.Authorization;
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
		services.AddCoreInfrastructure(configuration);
		services.AddEmailSender(configuration);
		services.AddDatabase(configuration);

		services.AddScoped<ICustomerAuthorizationService, CustomerAuthorizationService>();

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

		services
			.AddIdentityCore<Customer>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireUppercase = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequiredLength = 8;
				options.Lockout.AllowedForNewUsers = true;
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
				options.User.RequireUniqueEmail = true;
			})
			.AddRoles<CustomerRole>()
			.AddEntityFrameworkStores<CustomersDbContext>()
			.AddSignInManager()
			.AddDefaultTokenProviders();
	}
}
