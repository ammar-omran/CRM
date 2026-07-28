using FluentValidation;
using Microsoft.Extensions.Configuration;
using CRM.SharedKernel.Application.Extensions;
using Modules.Ticketing.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

public static class TicketingModuleRegistration
{
	public static IServiceCollection AddTicketingModule(this IServiceCollection services, IConfiguration configuration)
	{
		return services
			.AddTicketingModuleApi()
			.AddTicketingInfrastructure(configuration);
	}

	private static IServiceCollection AddTicketingModuleApi(this IServiceCollection services)
	{
		services.RegisterApiEndpointsFromAssemblyContaining(typeof(TicketingModuleRegistration));
		services.RegisterHandlersFromAssemblyContaining(typeof(TicketingModuleRegistration));
		services.AddValidatorsFromAssembly(typeof(TicketingModuleRegistration).Assembly);

		return services;
	}
}
