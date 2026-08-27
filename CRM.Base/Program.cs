using System.Text;
using CRM.Base.Modules.Api;
using CRM.Base.Modules.Routing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddModuleRegistration("1.0.0", "1.0.0");

// Register the dynamic YARP config provider before AddReverseProxy
builder.Services.AddSingleton<IProxyConfigProvider, DynamicProxyConfigProvider>();

builder.Services.AddReverseProxy();

builder.Services
			.AddEndpointsApiExplorer()
			.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = $"CRM.Base.API",
					Version = "v1"
				});
			});

builder.Services.AddRazorPages(options =>
{
    // Serve the Super Admin panel at the site root as the entry point.
    options.Conventions.AddPageRoute("/Admin/Index", "/");
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = builder.Configuration["AuthConfiguration:Issuer"],
				ValidAudience = builder.Configuration["AuthConfiguration:Audience"],
				IssuerSigningKey = new SymmetricSecurityKey(
							Encoding.UTF8.GetBytes(builder.Configuration["AuthConfiguration:Key"]
									?? throw new InvalidOperationException("JWT key is not configured")))
			};
		});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("Authenticated", policy =>
			policy.RequireAuthenticatedUser());
});

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAngularApp", policy =>
	{
		policy.AllowAnyOrigin()
					.AllowAnyHeader()
					.AllowAnyMethod();
	});
});

var app = builder.Build();

// Initialize SQLite database
await app.InitializePlatformDatabaseAsync();

// Load registered modules from database into runtime catalog
await app.LoadModuleCatalogAsync();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapModuleEndpoints();
app.MapInternalEndpoints();
app.MapReverseProxy();
app.MapRazorPages();

await app.RunAsync();
