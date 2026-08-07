using CRM.SharedKernel.Application.API.ErrorHandling;
using CRM.SharedKernel.Application.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTicketingModule(builder.Configuration);
builder.Services.AddSwagger();
builder.Services.AddHttpContextAccessor();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>().AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseModuleMiddlewares();

app.MapApiEndpoints();
app.MapDefaultEndpoints();

await app.RunAsync();
