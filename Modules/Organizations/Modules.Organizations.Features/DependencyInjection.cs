using Microsoft.Extensions.Configuration;
using FluentValidation;
using CRM.SharedKernel.Application;
using CRM.SharedKernel.Application.Extensions;
using CRM.SharedKernel.Domain.Events;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class OrganizationsModuleRegistration
{
	public static IServiceCollection AddOrganizationsModule(this IServiceCollection services, IConfiguration configuration)
	{
		return services
			.AddOrganizationsInfrastructure(configuration)
			.AddOrganizationsModuleApi(configuration);
	}

	private static IServiceCollection AddOrganizationsModuleApi(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddScoped<IEventPublisher, EventPublisher>();
		services.AddEmailSender(configuration);
		services.AddModuleTracing("organizations", "/api/organizations", "/api/agents");
		services.RegisterApiEndpointsFromAssemblyContaining(typeof(OrganizationsModuleRegistration));
		services.RegisterHandlersFromAssemblyContaining(typeof(OrganizationsModuleRegistration));
		services.AddValidatorsFromAssembly(typeof(OrganizationsModuleRegistration).Assembly);

		return services;
	}
}
