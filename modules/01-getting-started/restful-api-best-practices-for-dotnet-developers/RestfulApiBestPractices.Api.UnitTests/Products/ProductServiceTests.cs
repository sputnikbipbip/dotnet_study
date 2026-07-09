using AwesomeAssertions;
using Microsoft.AspNetCore.OutputCaching;
using RestfulApiBestPractices.Api.DTOs;
using RestfulApiBestPractices.Api.Services;
using Xunit;

namespace RestfulApiBestPractices.Api.UnitTests;

public class ProductServiceTests
{
    private static readonly ProductService _productService = new();
    
    [Fact]
    public async Task GetByIdAsync_ReturnsProduct()
    {
        //Arrange
        var product = await _productService.CreateAsync(
            new CreateProductRequest(
                "Laptop",
                "Test product only for testing",
                99.99m,
                100,
                "Test category"
            ));

        //Act
        var result = await _productService.GetByIdAsync(product.Id);
        
        //Assert
        result.Should().BeSameAs(product);
    }

    [Fact]
    public async Task UpdateAsync_InvalidProductId()
    {
        //Arrange
        int invalidProductId = 0;
        var request = new UpdateProductRequest(
            "Test Product",
            "Test Description",
            99.99m,
            100,
            "Test Category");
        
        //Act
        var product = await _productService.UpdateAsync(invalidProductId, request);

        //Assert
        product.Should().BeNull();
    }
}