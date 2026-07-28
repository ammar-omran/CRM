using CRM.SharedKernel.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// core registering services
builder.Services.AddCoreWebApiInfrastructure("Modules.Users");

// Module registration
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseModuleMiddlewares();
app.MapDefaultEndpoints();
app.MapApiEndpoints();

await app.RunAsync();
