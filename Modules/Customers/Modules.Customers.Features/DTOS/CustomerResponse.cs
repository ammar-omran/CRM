namespace Modules.Customers.Features.DTOS;

public record CustomerResponse(
    int Id,
    string Name,
    string Email,
    string Phone,
    DateTime CreatedAt
);
