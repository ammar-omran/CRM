namespace Modules.Customers.Domain.Authentication;

public sealed record CustomerLoginResponse(string Token, string RefreshToken);
