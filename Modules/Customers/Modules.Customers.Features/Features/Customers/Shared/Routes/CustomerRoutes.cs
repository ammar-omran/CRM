namespace Modules.Customers.Features.Customers.Shared.Routes;

public static class CustomerRoutes
{
	public const string Base = "api/customers";
	public const string GetById = $"{Base}/{{customerId:int}}";
	public const string Login = $"{Base}/login";
	public const string Register = $"{Base}/register";
	public const string GetAll = Base;
}
