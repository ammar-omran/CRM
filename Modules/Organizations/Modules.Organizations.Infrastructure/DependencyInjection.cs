using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.Organizations.Domain.Repositories;
using Modules.Organizations.Infrastructure.Database;
using Modules.Organizations.Infrastructure.Repositories;
using CRM.SharedKernel.Infrastructure.Database;
using Modules.Organizations.Domain.Modules;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
	public static IServiceCollection AddOrganizationsInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddCoreInfrastructure(configuration, "organizations");
		services.AddModuleManifest<OrganizationsModuleManifest>();

		services.AddDatabase(configuration);

		services.AddHttpClient("Modules.CustomersApi");
		services.AddScoped<ICustomerRepository, CustomerRepository>();
		services.AddScoped<IUserCommentRepository, UserCommentRepository>();

		return services;
	}

	private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("UserManagementDb");

		services.AddDbContext<OrganizationsDbContext>(options =>
		{
			options.UseSqlServer(connectionString, sqlOptions =>
			{
				sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "org");
			});
		});

		services.AddScoped<IApplicationDbContext>(sp =>
			sp.GetRequiredService<OrganizationsDbContext>());

		services.AddSingleton<AuditableInterceptor>();

		services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
		services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
	}
}
