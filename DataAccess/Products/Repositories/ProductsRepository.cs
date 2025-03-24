// DataAccess/Products/Repositories/ProductsRepository.cs
using Application.Products.Abstractions;
using Domain.Products.Models;
using Domain.Products.Requests;
using Dapper;
using System.Data;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Text;

namespace DataAccess.Products.Repositories
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public ProductsRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<Product> CreateProductAsync(ProductCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Insert the new product
                var insertQuery = @"
                    INSERT INTO [Products] (
                        Name, Description, Category, PremiseId, ManufacturerId, PermitId,
                        RegistrationNumber, ComplianceStatus, ImageBase64, CreatedAt, UpdatedAt
                    )
                    VALUES (
                        @Name, @Description, @Category, @PremiseId, @ManufacturerId, @PermitId,
                        @RegistrationNumber, @ComplianceStatus, @ImageBase64, @CreatedAt, @UpdatedAt
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    Name = request.Name,
                    Description = request.Description,
                    Category = request.Category,
                    PremiseId = request.PremiseId,
                    ManufacturerId = request.ManufacturerId,
                    PermitId = request.PermitId,
                    RegistrationNumber = request.RegistrationNumber,
                    ComplianceStatus = "Pending", // Default status
                    ImageBase64 = request.ImageBase64,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null
                };

                var productId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                // Return the created product object
                return new Product
                {
                    ProductId = productId,
                    Name = request.Name,
                    Description = request.Description,
                    Category = request.Category,
                    PremiseId = request.PremiseId,
                    ManufacturerId = request.ManufacturerId,
                    PermitId = request.PermitId,
                    RegistrationNumber = request.RegistrationNumber,
                    ComplianceStatus = "Pending",
                    ImageBase64 = request.ImageBase64,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt
                };
            }
        }

        public async Task<PagedResultResponse<Product>> GetProductsAsync(int pageNumber, int pageSize,int? premiseId, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                    SELECT *
                    FROM [Products]
                    WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                        AND (Name LIKE @Search
                        OR Description LIKE @Search
                        OR Category LIKE @Search)");
                }


                if (premiseId.HasValue &&  premiseId>0)
                {
                    query.Append(@"
                        AND (PremiseId= @PremiseId)");
                }

                query.Append(@"
                    ORDER BY CreatedAt DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [Products]
                    WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                        AND (Name LIKE @Search
                        OR Description LIKE @Search
                        OR Category LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    PremiseId= premiseId,
                    Search = $"%{search}%"
                }))
                {
                    var products = multi.Read<Product>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Product>
                    {
                        Items = products,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [Products]
                    WHERE ProductId = @ProductId";

                return await connection.QuerySingleOrDefaultAsync<Product>(query, new { ProductId = productId });
            }
        }

        public async Task<Product> UpdateProductAsync(ProductUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the product exists
                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [Products]
                    WHERE ProductId = @ProductId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { ProductId = request.ProductId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException(request.ProductId);
                }

                // Prepare the SQL query to update the product
                var updateQuery = @"
                    UPDATE [Products]
                    SET Name = COALESCE(@Name, Name),
                        Description = COALESCE(@Description, Description),
                        Category = COALESCE(@Category, Category),
                        PremiseId = COALESCE(@PremiseId, PremiseId),
                        ManufacturerId = COALESCE(@ManufacturerId, ManufacturerId),
                        PermitId = COALESCE(@PermitId, PermitId),
                        RegistrationNumber = COALESCE(@RegistrationNumber, RegistrationNumber),
                        ComplianceStatus = COALESCE(@ComplianceStatus, ComplianceStatus),
                        ImageBase64 = COALESCE(@ImageBase64, ImageBase64),
                        UpdatedAt = @UpdatedAt
                    WHERE ProductId = @ProductId";

                // Prepare the parameters
                var parameters = new
                {
                    ProductId = request.ProductId,
                    Name = request.Name,
                    Description = request.Description,
                    Category = request.Category,
                    PremiseId = request.PremiseId,
                    ManufacturerId = request.ManufacturerId,
                    PermitId = request.PermitId,
                    RegistrationNumber = request.RegistrationNumber,
                    ComplianceStatus = request.ComplianceStatus,
                    ImageBase64 = request.ImageBase64,
                    UpdatedAt = DateTime.UtcNow
                };

                // Execute the update
                await connection.ExecuteAsync(updateQuery, parameters);

                // Retrieve the updated product details
                var query = @"
                    SELECT *
                    FROM [Products]
                    WHERE ProductId = @ProductId";

                var product = await connection.QuerySingleOrDefaultAsync<Product>(query, new { ProductId = request.ProductId });

                if (product == null)
                {
                    throw new ItemDoesNotExistException(request.ProductId);
                }

                return product;
            }
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the product exists
                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [Products]
                    WHERE ProductId = @ProductId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { ProductId = productId });

                if (exists == 0)
                {
                    return false;
                }

                // Delete the product
                var deleteQuery = @"
                    DELETE FROM [Products]
                    WHERE ProductId = @ProductId";

                await connection.ExecuteAsync(deleteQuery, new { ProductId = productId });

                return true;
            }
        }

        public async Task<int> CountProductsAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [Products]";

                return await connection.ExecuteScalarAsync<int>(query);
            }
        }
    }
}