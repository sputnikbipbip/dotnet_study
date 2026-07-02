namespace RestfulApiBestPractices.Api.DTOs.Pagination;

public record PagedResponse<T>(
    IEnumerable<T> Data,
    PaginationMeta Pagination
);