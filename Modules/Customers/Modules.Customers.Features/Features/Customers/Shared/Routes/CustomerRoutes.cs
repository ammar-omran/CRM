namespace Modules.Customers.Features.Customers.Shared.Routes;

public static class CustomerRoutes
{
	public const string Base = "api/customers";
	public const string GetById = $"{Base}/{{customerId}}";
	public const string Login = $"{Base}/login";
	public const string Register = $"{Base}/register";
	public const string GetAll = Base;
	public const string ConfirmEmail = $"{Base}/confirm-email";
	public const string ResendOtp = $"{Base}/resend-otp";
	public const string Refresh = $"{Base}/refresh";
	public const string UpdatePassword = $"{Base}/update-password";
	public const string ResetPassword = $"{Base}/reset-password";
}
