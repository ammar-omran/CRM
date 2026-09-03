namespace Modules.Customers.Features.Customers.Shared;

public sealed record CustomerResponse(
    string Id,
    string Name,
    string Email,
    string PhoneNumber,
    string CountryCode,
    bool IsEmailVerified,
    DateTime CreatedAt
);
