using CRM.SharedKernel.Application.API.Requests;

namespace Modules.Users.Features.Organization.Shared.Requests;

public class GetOrganizationRequest : PaginationRequest
{
    public string? Name { get; set; }
}
