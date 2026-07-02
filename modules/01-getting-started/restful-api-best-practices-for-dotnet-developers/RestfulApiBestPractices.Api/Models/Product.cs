using RestfulApiBestPractices.Api.DTOs;

namespace RestfulApiBestPractices.Api.Models;

public record Product
{
    public int Id { get; init; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ProductResponse ToResponse() => new(
        Id, Name, Description, Price, Stock, Category, CreatedAt, UpdatedAt
    );
}
