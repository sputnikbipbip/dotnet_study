namespace RestfulApiBestPractices.Api.DTOs;

public record ProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string? Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);