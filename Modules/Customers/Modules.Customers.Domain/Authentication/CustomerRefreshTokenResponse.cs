namespace Modules.Customers.Domain.Authentication;

public sealed record CustomerRefreshTokenResponse(string Token, string RefreshToken);
