using CRM.SharedKernel.Application.API.Authorization;
using CRM.SharedKernel.Application.API.ErrorHandling;
using CRM.SharedKernel.Application.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomersModule(builder.Configuration);
builder.Services.AddSwagger();
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>().AddProblemDetails();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapControllers();
app.MapApiEndpoints();


// Default Portal-module permission management endpoints, mounted on the internal
// network boundary (Users owns the role data). Reachable only from the local
// network / Super Admin host, not exposed via the public gateway.
app.MapPortalPermissionEndpoints();

app.UseModuleMiddlewares();

await app.RunAsync();
