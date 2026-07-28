using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CRM.SharedKernel.Infrastructure.Configuration;
using CRM.SharedKernel.Infrastructure.Policies;
using Npgsql;
using OpenTelemetry.Resources;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Identity;
using CRM.SharedKernel.Infrastructure.Services;
using CRM.SharedKernel.Infrastructure.Database;
using CRM.SharedKernel.Domain.Modules;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
	public static IServiceCollection AddEmailSender(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
		services.AddScoped<IEmailSender, SmtpEmailSender>();

		return services;
	}
	public static IServiceCollection AddCoreInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration,
		string[] activityModuleNames)
	{
		services.AddMemoryCache();

		services.AddHostOpenTelemetry(activityModuleNames);

		services.AddJwtAuthentication(configuration);
		services.AddClaimsAuthorization();

		services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
		services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

		return services;
	}

	private static IServiceCollection AddHostOpenTelemetry(
		this IServiceCollection services,
		params string[] activityModuleNames)
	{
		services
			.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService("CRM"))
			.WithTracing(tracing =>
			{
				tracing
					.AddAspNetCoreInstrumentation()
					.AddHttpClientInstrumentation()
					.AddNpgsql()
					.AddSource(activityModuleNames);

				tracing.AddOtlpExporter();
			});

		services.AddHealthChecks()
		// Add a default liveness check to ensure app is responsive
		.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);


		return services;
	}

	private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<AuthConfiguration>()
			.Bind(configuration.GetSection(nameof(AuthConfiguration)));

		var tokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = configuration["AuthConfiguration:Issuer"],
			ValidAudience = configuration["AuthConfiguration:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AuthConfiguration:Key"] ?? throw new InvalidOperationException("JWT key is not configured")))
		};

		services.AddSingleton(tokenValidationParameters);

		services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = tokenValidationParameters;
			});

		return services;
	}

	private static IServiceCollection AddClaimsAuthorization(this IServiceCollection services)
	{
		services.AddSingleton<IConfigureOptions<AuthorizationOptions>, AuthorizationConfigureOptions>();
		services.AddAuthorization();

		return services;
	}

	public static IServiceCollection AddModuleManifest<TManifest>(
		this IServiceCollection services)
		where TManifest : class, IModuleManifest
	{
		services.AddSingleton<IModuleManifest, TManifest>();
		return services;
	}
}
