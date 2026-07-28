namespace Modules.Customers.Features.Customers.Shared;

public sealed record CustomerResponse(
    int Id,
    string Name,
    string Email,
    string PhoneNumber,
    string CountryCode,
    bool IsEmailVerified,
    DateTime CreatedDate
);
