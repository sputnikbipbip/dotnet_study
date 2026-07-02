using RestfulApiBestPractices.Api.DTOs;
using RestfulApiBestPractices.Api.Models;

namespace RestfulApiBestPractices.Api.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? category = null, string? sortBy = "id", bool descending = false);
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(CreateProductRequest request);
    Task<Product?> UpdateAsync(int id, UpdateProductRequest request);
    Task<Product?> PatchAsync(int id, PatchProductRequest request);
    Task<bool> DeleteAsync(int id);
}