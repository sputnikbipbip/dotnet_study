namespace RestfulApiBestPractices.Api.DTOs;

public record PatchProductRequest(
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    int? Stock = null,
    string? Category = null
);