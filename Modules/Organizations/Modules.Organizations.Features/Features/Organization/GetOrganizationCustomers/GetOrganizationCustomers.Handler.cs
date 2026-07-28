using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.API.Responses;
using CRM.SharedKernel.Infrastructure.Database;
using Modules.Organizations.Domain.Entities;
using Modules.Organizations.Features.Organization.Shared.Requests;
using Modules.Organizations.Features.Organization.Shared.Responses;

namespace Modules.Organizations.Features.Organization.GetOrganizationCustomers;

internal interface IGetOrganizationCustomersHandler : IHandler
{
    Task<Result<PaginationResponse<OrganizationCustomerResponse>>> HandleAsync(
        GetOrganizationCustomersRequest request,
        CancellationToken cancellationToken);
}

internal sealed class GetOrganizationCustomersHandler(
    IReadRepository<OrganizationCustomer> orgCustomerRepo,
    ILogger<GetOrganizationCustomersHandler> logger) : IGetOrganizationCustomersHandler
{
    public async Task<Result<PaginationResponse<OrganizationCustomerResponse>>> HandleAsync(
        GetOrganizationCustomersRequest request,
        CancellationToken ct)
    {
        logger.LogInformation("Getting Organization Customers");

        var query = orgCustomerRepo.Query;

        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(oc => oc.Customer.Name.Contains(request.Name));

        if (!string.IsNullOrWhiteSpace(request.Email))
            query = query.Where(oc => oc.Customer.Email.Contains(request.Email));

        if (!string.IsNullOrWhiteSpace(request.OrganizationName))
            query = query.Where(oc => oc.Organization.Name.Contains(request.OrganizationName));

        var total = request.SkipTotal ? -1 : await orgCustomerRepo.CountAsync(query, ct);

        query = query
            .OrderBy(oc => oc.Organization.Name)
            .ThenBy(oc => oc.Customer.Name)
            .Skip(request.Skip)
            .Take(request.Limit);

        var customers = await orgCustomerRepo.GetListAsync(
            query: query,
            selector: oc => new OrganizationCustomerResponse
            {
                Name = oc.Customer.Name,
                Email = oc.Customer.Email,
                Phone = oc.Customer.Phone,
                OrganizationName = oc.Organization.Name
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
