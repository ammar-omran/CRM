using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.API.Responses;
using CRM.SharedKernel.Infrastructure.Database;
using Modules.Organizations.Domain.Entities;
using Modules.Organizations.Features.Organization.Shared.Requests;
using Modules.Organizations.Features.Organization.Shared.Responses;

namespace Modules.Organizations.Features.Organization.GetOrganizations;

internal interface IGetOrganizationsHandler : IHandler
{
	Task<Result<PaginationResponse<OrganizationResponse>>> HandleAsync(GetOrganizationRequest request, CancellationToken cancellationToken);
}

internal sealed class GetOrganizationsHandler(
	IReadRepository<Domain.Entities.Organization> orgRepo,
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
