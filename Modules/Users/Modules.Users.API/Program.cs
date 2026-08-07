using CRM.SharedKernel.Application.API.ErrorHandling;
using CRM.SharedKernel.Application.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Module registration
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwagger();
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
app.MapDefaultEndpoints();
app.MapApiEndpoints();

await app.RunAsync();
