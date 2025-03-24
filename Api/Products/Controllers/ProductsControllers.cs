// Api/Products/Controllers/ProductsControllers.cs
using Domain.Products.Models;
using Domain.Products.Requests;
using Application.Products.Abstractions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Claims;
using DataAccess.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Products.Controllers
{
    public static class ProductsControllers
    {
        public static async Task<IResult> CreateProductAsync(
            IProductsRepository repo,
            [FromBody] ProductCreationRequest request,
            HttpContext httpContext)
        {
            try
            {
                // Ensure the user is authenticated
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create product with name: {Name}", request.Name);

                // Create the product
                var createdProduct = await repo.CreateProductAsync(request);

                Log.Information("Product created successfully with ID: {ProductId}", createdProduct.ProductId);
                return Results.Created($"/products/{createdProduct.ProductId}", createdProduct);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the product.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetProductsAsync(
            IProductsRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            int? premiseId=null,
            string? search = null)
        {
            try
            {
                Log.Information("Attempting to retrieve products with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                // Retrieve paginated products
                var pagedResult = await repo.GetProductsAsync(pageNumber, pageSize,premiseId, search);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No products found." });
                }

                Log.Information("Successfully retrieved {ProductCount} products out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving products.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetProductByIdAsync(
            IProductsRepository repo,
            int productId)
        {
            try
            {
                Log.Information("Attempting to retrieve product with ID: {ProductId}", productId);

                // Retrieve the product by ID
                var product = await repo.GetProductByIdAsync(productId);

                if (product == null)
                {
                    Log.Warning("Product with ID: {ProductId} not found.", productId);
                    return Results.NotFound(new { message = "Product not found." });
                }

                Log.Information("Successfully retrieved product with ID: {ProductId}", productId);
                return Results.Ok(product);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the product.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateProductAsync(
            IProductsRepository repo,
            [FromBody] ProductUpdateRequest request)
        {
            try
            {
                Log.Information("Attempting to update product with ID: {ProductId}", request.ProductId);

                // Update the product
                var updatedProduct = await repo.UpdateProductAsync(request);

                Log.Information("Product updated successfully with ID: {ProductId}", updatedProduct.ProductId);
                return Results.Ok(updatedProduct);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Product update failed - product does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the product.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteProductAsync(
            IProductsRepository repo,
            int productId)
        {
            try
            {
                Log.Information("Attempting to delete product with ID: {ProductId}", productId);

                // Delete the product
                var deleted = await repo.DeleteProductAsync(productId);

                if (deleted)
                {
                    Log.Information("Product deleted successfully with ID: {ProductId}", productId);
                    return Results.Ok(new { message = "Product deleted successfully." });
                }
                else
                {
                    Log.Warning("Product deletion failed - product does not exist.");
                    return Results.NotFound(new { message = "Product not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the product.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountProductsAsync(
            IProductsRepository repo)
        {
            try
            {
                Log.Information("Attempting to count all products.");

                // Count the products
                var count = await repo.CountProductsAsync();

                Log.Information("Successfully counted {ProductCount} products.", count);
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting products.");
                return Results.Problem(ex.Message);
            }
        }
    }
}