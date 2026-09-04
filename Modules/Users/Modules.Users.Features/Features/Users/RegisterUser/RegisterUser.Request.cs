namespace Modules.Users.Features.Users.RegisterUser;

public sealed record RegisterUserRequest(string Email, string Name, string? Password, string? Phone, string? Role);
