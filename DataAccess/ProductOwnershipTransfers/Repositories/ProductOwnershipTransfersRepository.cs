// DataAccess/ProductOwnershipTransfers/Repositories/ProductOwnershipTransfersRepository.cs
using Application.ProductOwnershipTransfers.Abstractions;
using Domain.ProductOwnershipTransfers.Models;
using Domain.ProductOwnershipTransfers.Requests;
using Dapper;
using System.Data;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Text;

namespace DataAccess.ProductOwnershipTransfers.Repositories
{
    public class ProductOwnershipTransfersRepository : IProductOwnershipTransfersRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public ProductOwnershipTransfersRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<ProductOwnershipTransfer> CreateTransferAsync(TransferCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Insert the new transfer
                var insertQuery = @"
                    INSERT INTO [ProductOwnershipTransfers] (
                        ProductId, ProductName, ProductType, ProductDescription,
                        FromPremiseId, FromPremiseName, FromPremiseAddress,
                        ToPremiseId, ToPremiseName, ToPremiseAddress,
                        IsRecipientExternal, Status, InitiatedAt, Comments
                    )
                    VALUES (
                        @ProductId, @ProductName, @ProductType, @ProductDescription,
                        @FromPremiseId, @FromPremiseName, @FromPremiseAddress,
                        @ToPremiseId, @ToPremiseName, @ToPremiseAddress,
                        @IsRecipientExternal, @Status, @InitiatedAt, @Comments
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    ProductId = request.ProductId,
                    ProductName = request.ProductName,
                    ProductType = request.ProductType,
                    ProductDescription = request.ProductDescription,
                    FromPremiseId = request.FromPremiseId,
                    FromPremiseName = request.FromPremiseName,
                    FromPremiseAddress = request.FromPremiseAddress,
                    ToPremiseId = request.ToPremiseId,
                    ToPremiseName = request.ToPremiseName,
                    ToPremiseAddress = request.ToPremiseAddress,
                    IsRecipientExternal = request.IsRecipientExternal,
                    Status = "Pending",
                    InitiatedAt = DateTime.UtcNow,
                    Comments = request.Comments
                };

                var transferId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                // Return the created transfer object
                return new ProductOwnershipTransfer
                {
                    TransferId = transferId,
                    ProductId = request.ProductId,
                    ProductName = request.ProductName,
                    ProductType = request.ProductType,
                    ProductDescription = request.ProductDescription,
                    FromPremiseId = request.FromPremiseId,
                    FromPremiseName = request.FromPremiseName,
                    FromPremiseAddress = request.FromPremiseAddress,
                    ToPremiseId = request.ToPremiseId,
                    ToPremiseName = request.ToPremiseName,
                    ToPremiseAddress = request.ToPremiseAddress,
                    IsRecipientExternal = request.IsRecipientExternal,
                    Status = "Pending",
                    InitiatedAt = DateTime.UtcNow,
                    Comments = request.Comments
                };
            }
        }

        public async Task<PagedResultResponse<ProductOwnershipTransfer>> GetTransfersAsync(int pageNumber, int pageSize,int? productId, int? premiseId, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                    SELECT *
                    FROM [ProductOwnershipTransfers]
                    WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                        AND (ProductName LIKE @Search
                        OR FromPremiseName LIKE @Search
                        OR ToPremiseName LIKE @Search)");
                }

                if (productId.HasValue)
                {
                    query.Append(@"
                        AND (ProductId= @ProductId)");
                }

                if (premiseId.HasValue)
                {
                    query.Append(@"
                        AND (FromPremiseId= @PremiseId OR ToPremiseId= @PremiseId )");
                }

                query.Append(@"
                    ORDER BY InitiatedAt DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [ProductOwnershipTransfers]
                    WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                        AND (ProductName LIKE @Search
                        OR FromPremiseName LIKE @Search
                        OR ToPremiseName LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    ProductId=productId,
                    PremiseId=premiseId
                }))
                {
                    var transfers = multi.Read<ProductOwnershipTransfer>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<ProductOwnershipTransfer>
                    {
                        Items = transfers,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<ProductOwnershipTransfer?> GetTransferByIdAsync(int transferId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [ProductOwnershipTransfers]
                    WHERE TransferId = @TransferId";

                return await connection.QuerySingleOrDefaultAsync<ProductOwnershipTransfer>(query, new { TransferId = transferId });
            }
        }

        public async Task<ProductOwnershipTransfer> UpdateTransferAsync(TransferUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the transfer exists
                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [ProductOwnershipTransfers]
                    WHERE TransferId = @TransferId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { TransferId = request.TransferId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException(request.TransferId);
                }

                // Prepare the SQL query to update the transfer
                var updateQuery = @"
                    UPDATE [ProductOwnershipTransfers]
                    SET Status = @Status,
                        Comments = @Comments,
                        ApprovedAt = CASE WHEN @Status = 'Approved' THEN @UpdatedAt ELSE ApprovedAt END,
                        RejectedAt = CASE WHEN @Status = 'Rejected' THEN @UpdatedAt ELSE RejectedAt END
                    WHERE TransferId = @TransferId";

                // Prepare the parameters
                var parameters = new
                {
                    TransferId = request.TransferId,
                    Status = request.Status,
                    Comments = request.Comments,
                    UpdatedAt = DateTime.UtcNow
                };

                // Execute the update
                await connection.ExecuteAsync(updateQuery, parameters);

                // Retrieve the updated transfer details
                var query = @"
                    SELECT *
                    FROM [ProductOwnershipTransfers]
                    WHERE TransferId = @TransferId";

                var transfer = await connection.QuerySingleOrDefaultAsync<ProductOwnershipTransfer>(query, new { TransferId = request.TransferId });

                if (transfer == null)
                {
                    throw new ItemDoesNotExistException(request.TransferId);
                }

                return transfer;
            }
        }

        public async Task<bool> DeleteTransferAsync(int transferId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the transfer exists
                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [ProductOwnershipTransfers]
                    WHERE TransferId = @TransferId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { TransferId = transferId });

                if (exists == 0)
                {
                    return false;
                }

                // Delete the transfer
                var deleteQuery = @"
                    DELETE FROM [ProductOwnershipTransfers]
                    WHERE TransferId = @TransferId";

                await connection.ExecuteAsync(deleteQuery, new { TransferId = transferId });

                return true;
            }
        }

        public async Task<int> CountTransfersAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [ProductOwnershipTransfers]";

                return await connection.ExecuteScalarAsync<int>(query);
            }
        }
    }
}