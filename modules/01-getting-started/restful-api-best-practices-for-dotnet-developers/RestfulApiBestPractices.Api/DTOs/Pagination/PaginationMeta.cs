namespace RestfulApiBestPractices.Api.DTOs.Pagination;

public record PaginationMeta(
    int Page,
    int PageSize,
    int TotalPages,
    int TotalCount,
    bool HasNext,
    bool HasPrevious
);