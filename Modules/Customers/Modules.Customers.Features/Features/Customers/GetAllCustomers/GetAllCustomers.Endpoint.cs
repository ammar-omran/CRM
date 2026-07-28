using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CRM.SharedKernel.API.Abstractions;
using CRM.SharedKernel.API.Extensions;
using Modules.Customers.Features.Customers.Shared;
using Modules.Customers.Features.Customers.Shared.Routes;

namespace Modules.Customers.Features.Customers.GetAllCustomers;

public class GetAllCustomersEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapGet(CustomerRoutes.GetAll, Handle)
			.RequireAuthorization("customers:view");
	}

	private static async Task<IResult> Handle(
		int? skip,
		int? limit,
		IGetAllCustomersHandler handler,
		CancellationToken cancellationToken)
	{
		var request = new GetAllCustomersRequest(
			Skip: skip ?? 0,
			Limit: limit ?? 20);

		var response = await handler.HandleAsync(request, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.Ok(response.Value);
	}
}
