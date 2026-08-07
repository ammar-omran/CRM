using CRM.SharedKernel.Domain.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

// ReSharper disable once CheckNamespace
namespace CRM.SharedKernel.Application.API.Extensions;

public static class SupportSwagger
{
	public static IServiceCollection AddSwagger(this IServiceCollection services)
	{
		var provider = services.BuildServiceProvider();
		var manifest = provider.GetService<IModuleManifest>();
		services
			.AddEndpointsApiExplorer()
			.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = $"{manifest.Identity.ModuleId}.api",
					Version = "v1"
				});

				options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					In = ParameterLocation.Header,
					Description = "JWT Authorization header. Enter: {token} (without Bearer)",
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					BearerFormat = "JWT",
					Scheme = "Bearer"
				});

				options.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type=ReferenceType.SecurityScheme,
								Id="Bearer"
							}
						},
						Array.Empty<string>()
					}
				});
			});


		return services;
	}
}
