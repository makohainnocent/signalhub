// Api/Products/EndPointDefinations/ProductsEndpoints.cs
using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.Products.Abstractions;
using Domain.Products.Requests;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Api.Products.Controllers;

namespace Api.Products.EndPointDefinations
{
    public class ProductsEndpoints : IEndpointDefinition
    {
        public void RegisterEndpoints(WebApplication app)
        {
            ApiVersionSet apiVersionSet = app.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1))
                .ReportApiVersions()
                .Build();

            RouteGroupBuilder versionedGroup = app
                .MapGroup("/api/v{apiVersion:apiVersion}")
                .WithApiVersionSet(apiVersionSet);

            var products = versionedGroup.MapGroup("/products")
                .WithTags("Products Management");

            // Create a new product
            products.MapPost("/", async (IProductsRepository repo, [FromBody] ProductCreationRequest request, HttpContext httpContext) =>
            {
                return await ProductsControllers.CreateProductAsync(repo, request, httpContext);
            })
            .RequireAuthorization();

            // Get all products (paginated)
            products.MapGet("/", async (IProductsRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? premiseId = null, [FromQuery] string? search = null) =>
            {
                return await ProductsControllers.GetProductsAsync(repo, pageNumber, pageSize,premiseId, search);
            });

            // Get a product by ID
            products.MapGet("/{productId:int}", async (IProductsRepository repo, int productId) =>
            {
                return await ProductsControllers.GetProductByIdAsync(repo, productId);
            });

            // Update a product
            products.MapPut("/", async (IProductsRepository repo, [FromBody] ProductUpdateRequest request) =>
            {
                return await ProductsControllers.UpdateProductAsync(repo, request);
            })
            .RequireAuthorization();

            // Delete a product
            products.MapDelete("/{productId:int}", async (IProductsRepository repo, int productId) =>
            {
                return await ProductsControllers.DeleteProductAsync(repo, productId);
            })
            .RequireAuthorization();

            // Count all products
            products.MapGet("/count", async (IProductsRepository repo) =>
            {
                return await ProductsControllers.CountProductsAsync(repo);
            });
        }
    }
}