using CRM.SharedKernel.Application.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCoreWebApiInfrastructure("Modules.Ticketing");
builder.Services.AddTicketingModule(builder.Configuration);
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

app.MapApiEndpoints();
app.MapDefaultEndpoints();

await app.RunAsync();
