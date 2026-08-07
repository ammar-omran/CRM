using CRM.SharedKernel.Application.API.Requests;

namespace Modules.Users.Features.Organization.Shared.Requests;

public class GetOrganizationCustomersRequest : PaginationRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? OrganizationName { get; set; }
}
