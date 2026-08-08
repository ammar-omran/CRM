using FluentValidation;
using Microsoft.Extensions.Configuration;
using CRM.SharedKernel.Application.Extensions;
using CRM.SharedKernel.Application.API.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class CustomersModuleRegistration
{
	public static IServiceCollection AddCustomersModule(this IServiceCollection services, IConfiguration configuration)
	{
		return services
			.AddCustomersInfrastructure(configuration)
			.AddCustomersModuleApi();
	}

	private static IServiceCollection AddCustomersModuleApi(this IServiceCollection services)
	{
		services.RegisterApiEndpointsFromAssemblyContaining(typeof(CustomersModuleRegistration));
		services.AddSharedKernelModuleMiddlewares();
		services.RegisterModuleMiddlewaresFromAssemblyContaining(typeof(CustomersModuleRegistration));
		services.RegisterHandlersFromAssemblyContaining(typeof(CustomersModuleRegistration));
		services.AddValidatorsFromAssembly(typeof(CustomersModuleRegistration).Assembly);

		return services;
	}
}
