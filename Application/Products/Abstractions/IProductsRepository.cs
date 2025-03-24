// Application/Products/Abstractions/IProductsRepository.cs
using Domain.Common.Responses;
using Domain.Products.Models;
using Domain.Products.Requests;

namespace Application.Products.Abstractions
{
    public interface IProductsRepository
    {
        Task<Product> CreateProductAsync(ProductCreationRequest request);
        Task<PagedResultResponse<Product>> GetProductsAsync(int pageNumber, int pageSize, int? premiseId, string? search = null);
        Task<Product?> GetProductByIdAsync(int productId);
        Task<Product> UpdateProductAsync(ProductUpdateRequest request);
        Task<bool> DeleteProductAsync(int productId);
        Task<int> CountProductsAsync();
    }
}