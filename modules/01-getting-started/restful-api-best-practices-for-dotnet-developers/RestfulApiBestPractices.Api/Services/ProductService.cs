using RestfulApiBestPractices.Api.DTOs;
using RestfulApiBestPractices.Api.Models;

namespace RestfulApiBestPractices.Api.Services;

public class ProductService : IProductService
{
    private static readonly List<Product> _products =
    [
        new Product
        {
            Id = 1, Name = "Laptop", Description = "High-performance laptop for developers", Price = 999.99m,
            Stock = 50, Category = "Electronics"
        },
        new Product
        {
            Id = 2, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with precision tracking",
            Price = 29.99m, Stock = 200, Category = "Electronics"
        },
        new Product
        {
            Id = 3, Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard with Cherry MX switches",
            Price = 149.99m, Stock = 100, Category = "Electronics"
        },
        new Product
        {
            Id = 4, Name = "4K Monitor", Description = "27-inch 4K UHD monitor with HDR support", Price = 449.99m,
            Stock = 30, Category = "Electronics"
        },
        new Product
        {
            Id = 5, Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI and SD card reader", Price = 49.99m,
            Stock = 150, Category = "Accessories"
        },
        new Product
        {
            Id = 6, Name = "Webcam HD", Description = "1080p HD webcam with built-in microphone", Price = 79.99m,
            Stock = 80, Category = "Electronics"
        },
        new Product
        {
            Id = 7, Name = "Noise-Cancelling Headphones", Description = "Premium wireless headphones with ANC",
            Price = 299.99m, Stock = 45, Category = "Audio"
        },
        new Product
        {
            Id = 8, Name = "Standing Desk", Description = "Electric height-adjustable standing desk", Price = 599.99m,
            Stock = 20, Category = "Furniture"
        },
        new Product
        {
            Id = 9, Name = "Ergonomic Chair", Description = "Mesh ergonomic office chair with lumbar support",
            Price = 399.99m, Stock = 35, Category = "Furniture"
        },
        new Product
        {
            Id = 10, Name = "Desk Lamp", Description = "LED desk lamp with adjustable color temperature",
            Price = 39.99m, Stock = 120, Category = "Accessories"
        }
    ];

    private static int _nextId = 11;
    private static readonly Lock _lock = new();

    public Task<IEnumerable<Product>> GetAllAsync()
        => Task.FromResult<IEnumerable<Product>>(_products);

    public Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? category = null, string? sortBy = "id", bool descending = false)
    {
        var query = _products.AsEnumerable();

        // Filter by category
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true);
        }

        // Sort
        query = sortBy?.ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "createdat" => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "stock" => descending ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
            _ => descending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
        };

        var totalCount = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize);

        return Task.FromResult((items, totalCount));
    }

    public Task<Product?> GetByIdAsync(int id)
        => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

    public Task<Product> CreateAsync(CreateProductRequest request)
    {
        lock (_lock)
        {
            var product = new Product
            {
                Id = _nextId++,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                Category = request.Category
            };
            _products.Add(product);
            return Task.FromResult(product);
        }
    }

    public Task<Product?> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return Task.FromResult<Product?>(null);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.Category = request.Category;
        product.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult<Product?>(product);
    }

    public Task<Product?> PatchAsync(int id, PatchProductRequest request)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return Task.FromResult<Product?>(null);

        if (request.Name is not null) product.Name = request.Name;
        if (request.Description is not null) product.Description = request.Description;
        if (request.Price.HasValue) product.Price = request.Price.Value;
        if (request.Stock.HasValue) product.Stock = request.Stock.Value;
        if (request.Category is not null) product.Category = request.Category;
        product.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult<Product?>(product);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return Task.FromResult(false);

        _products.Remove(product);
        return Task.FromResult(true);
    }
}