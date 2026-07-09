using Microsoft.AspNetCore.Mvc;
using RestfulApiBestPractices.Api.DTOs;
using RestfulApiBestPractices.Api.DTOs.Pagination;
using RestfulApiBestPractices.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("Expires30", builder => 
        builder.Expire(TimeSpan.FromSeconds(30)));
});

var app = builder.Build();

// Middleware
app.UseExceptionHandler();
app.MapOpenApi();
app.MapScalarApiReference();
app.UseOutputCache();

// API Routes - Version 1
var api = app.MapGroup("/api/v1");

// GET /api/v1/products - Get all products (paginated)
api.MapGet("/products", async (
    int page = 1,
    int pageSize = 20,
    string? category = null,
    string? sort = "id",
    string order = "asc",
    IProductService service = default!) =>
{
    pageSize = Math.Clamp(pageSize, 1, 100);
    var (products, totalCount) = await service.GetPagedAsync(page, pageSize, category, sort, order == "desc");
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    return Results.Ok(new PagedResponse<ProductResponse>(
        products.Select(p => p.ToResponse()),
        new PaginationMeta(page, pageSize, totalPages, totalCount, page < totalPages, page > 1)
    ));
})
.WithName("GetProducts")
.WithSummary("Get all products with pagination, filtering, and sorting")
.WithDescription("Returns a paginated list of products. Supports filtering by category and sorting by id, name, price, or createdAt.")
.Produces<PagedResponse<ProductResponse>>(StatusCodes.Status200OK)
.WithTags("Products");

// GET /api/v1/products/{id} - Get a product by ID
api.MapGet("/products/{id:int}", async (int id, IProductService service) =>
{
    var product = await service.GetByIdAsync(id);

    //verify output cache
    Thread.Sleep(TimeSpan.FromSeconds(2));
    
    return product is null
        ? Results.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Product Not Found",
            Detail = $"Product with ID {id} was not found.",
            Instance = $"/api/v1/products/{id}"
        })
        : Results.Ok(product.ToResponse());
})
.WithName("GetProductById")
.WithSummary("Get a product by ID")
.WithDescription("Returns a single product based on its unique identifier. Returns 404 if the product doesn't exist.")
.Produces<ProductResponse>(StatusCodes.Status200OK)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
.WithTags("Products")
.CacheOutput("Expires30");

// POST /api/v1/products - Create a new product
api.MapPost("/products", async (CreateProductRequest request, IProductService service) =>
{
    // Basic validation
    var errors = new Dictionary<string, string[]>();
    if (string.IsNullOrWhiteSpace(request.Name))
        errors["name"] = ["Name is required."];
    if (request.Name?.Length > 200)
        errors["name"] = ["Name must be 200 characters or less."];
    if (string.IsNullOrWhiteSpace(request.Description))
        errors["description"] = ["Description is required."];
    if (request.Price <= 0)
        errors["price"] = ["Price must be greater than 0."];
    if (request.Stock < 0)
        errors["stock"] = ["Stock cannot be negative."];

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var product = await service.CreateAsync(request);
    return Results.Created($"/api/v1/products/{product.Id}", product.ToResponse());
})
.WithName("CreateProduct")
.WithSummary("Create a new product")
.WithDescription("Creates a new product and returns it with a 201 Created status and Location header.")
.Produces<ProductResponse>(StatusCodes.Status201Created)
.Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
.WithTags("Products");

// PUT /api/v1/products/{id} - Update a product (full replacement)
api.MapPut("/products/{id:int}", async (int id, UpdateProductRequest request, IProductService service) =>
{
    var product = await service.UpdateAsync(id, request);
    return product is null
        ? Results.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Product Not Found",
            Detail = $"Product with ID {id} was not found.",
            Instance = $"/api/v1/products/{id}"
        })
        : Results.NoContent();
})
.WithName("UpdateProduct")
.WithSummary("Update an existing product (full replacement)")
.WithDescription("Replaces all fields of an existing product. All fields must be provided.")
.Produces(StatusCodes.Status204NoContent)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
.WithTags("Products");

// PATCH /api/v1/products/{id} - Partially update a product
api.MapPatch("/products/{id:int}", async (int id, PatchProductRequest request, IProductService service) =>
{
    var product = await service.PatchAsync(id, request);
    return product is null
        ? Results.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Product Not Found",
            Detail = $"Product with ID {id} was not found.",
            Instance = $"/api/v1/products/{id}"
        })
        : Results.NoContent();
})
.WithName("PatchProduct")
.WithSummary("Partially update a product")
.WithDescription("Updates only the provided fields of a product. Omitted fields remain unchanged.")
.Produces(StatusCodes.Status204NoContent)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
.WithTags("Products");

// DELETE /api/v1/products/{id} - Delete a product
api.MapDelete("/products/{id:int}", async (int id, IProductService service) =>
{
    var deleted = await service.DeleteAsync(id);
    return deleted
        ? Results.NoContent()
        : Results.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Product Not Found",
            Detail = $"Product with ID {id} was not found.",
            Instance = $"/api/v1/products/{id}"
        });
})
.WithName("DeleteProduct")
.WithSummary("Delete a product")
.WithDescription("Removes a product from the system. Returns 204 on success, 404 if not found.")
.Produces(StatusCodes.Status204NoContent)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
.WithTags("Products");

// Nested resource example: GET /api/v1/products/{id}/reviews
api.MapGet("/products/{productId:int}/reviews", async (int productId, IProductService service) =>
{
    var product = await service.GetByIdAsync(productId);
    if (product is null)
    {
        return Results.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Product Not Found",
            Detail = $"Product with ID {productId} was not found.",
            Instance = $"/api/v1/products/{productId}/reviews"
        });
    }

    // Return mock reviews (in a real app, this would be a separate service)
    var reviews = new[]
    {
        new { Id = 1, ProductId = productId, Rating = 5, Comment = "Excellent product!", Author = "John Doe", CreatedAt = DateTime.UtcNow.AddDays(-10) },
        new { Id = 2, ProductId = productId, Rating = 4, Comment = "Good value for money.", Author = "Jane Smith", CreatedAt = DateTime.UtcNow.AddDays(-5) }
    };

    return Results.Ok(reviews);
})
.WithName("GetProductReviews")
.WithSummary("Get reviews for a product")
.WithDescription("Returns all reviews for a specific product. Demonstrates nested resource pattern.")
.WithTags("Products");

app.Run();