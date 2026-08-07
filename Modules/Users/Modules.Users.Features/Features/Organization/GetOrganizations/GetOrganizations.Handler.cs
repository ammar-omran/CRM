using CRM.SharedKernel.Application.API.Responses;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Interfaces;
using CRM.SharedKernel.Domain.Results;
using Microsoft.Extensions.Logging;
using Modules.Users.Features.Organization.Shared.Errors;
using Modules.Users.Features.Organization.Shared.Requests;
using Modules.Users.Features.Organization.Shared.Responses;

namespace Modules.Users.Features.Organization.GetOrganizations;

internal interface IGetOrganizationsHandler : IHandler
{
	Task<Result<PaginationResponse<OrganizationResponse>>> HandleAsync(GetOrganizationRequest request, CancellationToken cancellationToken);
}

internal sealed class GetOrganizationsHandler(
	IReadRepository<Domain.OrganizationAggregate.Organization> orgRepo,
	ILogger<GetOrganizationsHandler> logger) : IGetOrganizationsHandler
{
	public async Task<Result<PaginationResponse<OrganizationResponse>>> HandleAsync(
		GetOrganizationRequest request,
		CancellationToken ct)
	{
		logger.LogInformation("Getting Organizations");

		var query = orgRepo.Query;

		if (!string.IsNullOrWhiteSpace(request.Name))
			query = query.Where(o => o.Name == request.Name);

		var total = request.SkipTotal ? -1 : await orgRepo.CountAsync(query, ct);

		query = query
			.OrderBy(o => o.CreatedAt)
			.Skip(request.Skip)
			.Take(request.Limit);

		var orgs = await orgRepo.GetListAsync(
			query: query,
			selector: o =>
				new OrganizationResponse
				{
					Id = o.Id,
					Name = o.Name,
					CreatedAt = o.CreatedAt,
					AgentsCount = o.OrganizationAgents.Count,
					CustomersCount = o.OrganizationCustomers.Count,
				},
			cancellation: ct);

		var result = new PaginationResponse<OrganizationResponse>(
			orgs,
			request.Skip / request.Limit,
			request.Limit,
			total);

		logger.LogInformation("Retrieved {Count} Organizations", result.Items.Count);

		return result;
	}
}
