using FluentValidation;
using Microsoft.Extensions.Configuration;
using CRM.SharedKernel.Application.Extensions;
using CRM.SharedKernel.Application.API.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class TicketingModuleRegistration
{
	public static IServiceCollection AddTicketingModule(this IServiceCollection services, IConfiguration configuration)
	{
		return services
			.AddTicketingInfrastructure(configuration)
			.AddTicketingModuleApi();
	}

	private static IServiceCollection AddTicketingModuleApi(this IServiceCollection services)
	{
		services.RegisterApiEndpointsFromAssemblyContaining(typeof(TicketingModuleRegistration));
		services.AddSharedKernelModuleMiddlewares();
		services.RegisterModuleMiddlewaresFromAssemblyContaining(typeof(TicketingModuleRegistration));
		services.RegisterHandlersFromAssemblyContaining(typeof(TicketingModuleRegistration));
		services.AddValidatorsFromAssembly(typeof(TicketingModuleRegistration).Assembly);

		return services;
	}
}
