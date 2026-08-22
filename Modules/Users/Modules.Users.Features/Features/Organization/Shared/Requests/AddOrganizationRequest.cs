namespace Modules.Users.Features.Organization.Shared.Requests;

public class AddOrganizationRequest
{
	public string Name { get; set; } = string.Empty;
	public List<AddOrganizationRequestAgents> Agents { get; set; } = [];
	public List<AddOrganizationCustomerRequest> Customers { get; set; } = [];
}

public class AddOrganizationCustomerRequest
{
	public int Id { get; set; }
}

public class AddOrganizationRequestAgents
{
	public int Id { get; set; }
	public string Role { get; set; } = string.Empty;
}
