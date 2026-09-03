using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Application.API.Responses;
using CRM.SharedKernel.Infrastructure.Database;
using Modules.Users.Domain.OrganizationAggregate;
using Modules.Users.Features.Organizations.Shared.Requests;
using Modules.Users.Features.Organizations.Shared.Responses;
using CRM.SharedKernel.Domain.Interfaces;

namespace Modules.Users.Features.Organizations.GetOrganizationCustomers;

internal interface IGetOrganizationCustomersHandler : IHandler
{
	Task<Result<PaginationResponse<OrganizationCustomerResponse>>> HandleAsync(
			GetOrganizationCustomersRequest request,
			CancellationToken cancellationToken);
}

internal sealed class GetOrganizationCustomersHandler(
		IReadRepository<Customer> orgCustomerRepo,
		ILogger<GetOrganizationCustomersHandler> logger) : IGetOrganizationCustomersHandler
{
	public async Task<Result<PaginationResponse<OrganizationCustomerResponse>>> HandleAsync(
			GetOrganizationCustomersRequest request,
			CancellationToken ct)
	{
		logger.LogInformation("Getting Organization Customers");

		var query = orgCustomerRepo.Query;

		if (!string.IsNullOrWhiteSpace(request.Name))
			query = query.Where(c => c.Name.Contains(request.Name));

		if (!string.IsNullOrWhiteSpace(request.Email))
			query = query.Where(c => c.Email.Contains(request.Email));

		if (!string.IsNullOrWhiteSpace(request.OrganizationName))
			query = query.Where(c => c.Organization.Name.Contains(request.OrganizationName));

		var total = request.SkipTotal ? -1 : await orgCustomerRepo.CountAsync(query, ct);

		query = query
				.OrderBy(c => c.Organization.Name)
				.ThenBy(c => c.Name)
				.Skip(request.Skip)
				.Take(request.Limit);

		var customers = await orgCustomerRepo.GetListAsync(
				query: query,
				selector: c => new OrganizationCustomerResponse
				{
					Name = c.Name,
					Email = c.Email,
					Phone = c.Phone,
					OrganizationName = c.Organization.Name
				},
				cancellation: ct);

		var result = new PaginationResponse<OrganizationCustomerResponse>(
				customers,
				request.Skip / request.Limit,
				request.Limit,
				total);

		logger.LogInformation("Retrieved {Count} Organization Customers", result.Items.Count);

		return result;
	}
}
