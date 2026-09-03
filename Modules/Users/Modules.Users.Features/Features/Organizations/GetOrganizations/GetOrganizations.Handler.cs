using CRM.SharedKernel.Application.API.Responses;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Users.Features.Organizations.Shared.Requests;
using Modules.Users.Features.Organizations.Shared.Responses;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Organizations.GetOrganizations;

public interface IGetOrganizationsHandler : IHandler
{
	Task<Result<PaginationResponse<OrganizationResponse>>> HandleAsync(GetOrganizationRequest request, CancellationToken cancellationToken);
}

internal sealed class GetOrganizationsHandler(
	OrganizationsDbContext dbContext,
	ILogger<GetOrganizationsHandler> logger) : IGetOrganizationsHandler
{
	public async Task<Result<PaginationResponse<OrganizationResponse>>> HandleAsync(
		GetOrganizationRequest request,
		CancellationToken ct)
	{
		logger.LogInformation("Getting Organizations");

		var query = dbContext.Organizations.AsQueryable();

		if (!string.IsNullOrWhiteSpace(request.Name))
			query = query.Where(o => o.Name == request.Name);

		var total = request.SkipTotal ? -1 : await query.CountAsync(ct);

		var orgs = await query
			.OrderBy(o => o.CreatedAt)
			.Skip(request.Skip)
			.Take(request.Limit)
			.Select(o =>
				new OrganizationResponse
				{
					Id = o.Id,
					Name = o.Name,
					CreatedAt = o.CreatedAt,
					AgentsCount = o.OrganizationAgents.Count,
					CustomersCount = o.OrganizationCustomers.Count,
				})
			.ToListAsync(ct);

		var result = new PaginationResponse<OrganizationResponse>(
			orgs,
			request.Skip / request.Limit,
			request.Limit,
			total);

		logger.LogInformation("Retrieved {Count} Organizations", result.Items.Count);

		return result;
	}
}
