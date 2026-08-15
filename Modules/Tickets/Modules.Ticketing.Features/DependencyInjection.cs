using FluentValidation;
using Microsoft.Extensions.Configuration;
using CRM.SharedKernel.Application.Events;
using CRM.SharedKernel.Application.Extensions;
using CRM.SharedKernel.Application.API.Extensions;
using CRM.SharedKernel.Domain.Events;

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
		services.AddSharedKernelModuleMiddlewares();
		services.RegisterModuleMiddlewaresFromAssemblyContaining(typeof(TicketingModuleRegistration));
		services.RegisterHandlersFromAssemblyContaining(typeof(TicketingModuleRegistration));
		services.AddValidatorsFromAssembly(typeof(TicketingModuleRegistration).Assembly);

		// Module events: forward published events to the platform for delivery to subscribers.
		services.AddHttpClient();
		services.AddScoped<IModuleEventPublisher, ModuleEventPublisher>();

		return services;
	}
}
