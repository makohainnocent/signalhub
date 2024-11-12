using Application.Application.Abstractions;
using Dapper;
using Domain.FarmApplication.Requests;
using Domain.Core.Models;
using Domain.Common.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Abstractions;
using DataAccess.Common.Exceptions;
using Domain.DocumentManagement.Requests;
using Application.DocumentManagement.Abstractions;

namespace DataAccess.FarmApplication.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public DocumentRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<Document> CreateDocumentAsync(DocumentCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO [Document] (FarmId, UserId, AnimalId, Type, Owner, Description, DocumentString, CreatedAt, UpdatedAt)
                    VALUES (@FarmId, @UserId, @AnimalId, @Type, @Owner, @Description, @DocumentString, @CreatedAt, @UpdatedAt);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    FarmId = request.FarmId,
                    UserId = request.UserId,
                    AnimalId = request.AnimalId,
                    Type = request.Type,
                    Owner = request.Owner,
                    Description = request.Description,
                    DocumentString = request.DocumentString,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var documentId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new Document
                {
                    DocumentId = documentId,
                    FarmId = request.FarmId,
                    UserId = request.UserId,
                    AnimalId = request.AnimalId,
                    Type = request.Type,
                    Owner = request.Owner,
                    Description = request.Description,
                    DocumentString = request.DocumentString,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt
                };
            }
        }

        public async Task<Document?> GetDocumentByIdAsync(int documentId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [Document]
                    WHERE DocumentId = @DocumentId";

                return await connection.QuerySingleOrDefaultAsync<Document>(query, new { DocumentId = documentId });
            }
        }

        public async Task<PagedResultResponse<Document>> GetAllDocumentsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? userId = null,
            int? farmId = null,
            int? animalId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                    SELECT *
                    FROM [Document]
                    WHERE 1=1");

                if (userId!=0)
                    query.Append(" AND UserId = @UserId");

                if (farmId!=0)
                    query.Append(" AND FarmId = @FarmId");

                if (animalId!=0)
                    query.Append(" AND AnimalId = @AnimalId");

                if (!string.IsNullOrWhiteSpace(search))
                    query.Append(" AND (Type LIKE @Search OR Description LIKE @Search)");

                query.Append(@"
                    ORDER BY CreatedAt DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [Document]
                    WHERE 1=1");

                if (userId!=0)
                    query.Append(" AND UserId = @UserId");

                if (farmId!=0)
                    query.Append(" AND FarmId = @FarmId");

                if (animalId!=0)
                    query.Append(" AND AnimalId = @AnimalId");

                if (!string.IsNullOrWhiteSpace(search))
                    query.Append(" AND (Type LIKE @Search OR Description LIKE @Search)");

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    UserId = userId,
                    FarmId = farmId,
                    AnimalId = animalId
                }))
                {
                    var documents = multi.Read<Document>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Document>
                    {
                        Items = documents,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Document> UpdateDocumentAsync(int documentId, DocumentUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [Document]
                    WHERE DocumentId = @DocumentId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { DocumentId = documentId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException(documentId);
                }

                var updateQuery = @"
                    UPDATE [Document]
                    SET FarmId = @FarmId,
                        UserId = @UserId,
                        AnimalId = @AnimalId,
                        Type = @Type,
                        Owner = @Owner,
                        Description = @Description,
                        DocumentString = @DocumentString,
                        UpdatedAt = @UpdatedAt
                    WHERE DocumentId = @DocumentId";

                var parameters = new
                {
                    DocumentId = documentId,
                    FarmId = request.FarmId,
                    UserId = request.UserId,
                    AnimalId = request.AnimalId,
                    Type = request.Type,
                    Owner = request.Owner,
                    Description = request.Description,
                    DocumentString = request.DocumentString,
                    UpdatedAt = DateTime.UtcNow
                };

                await connection.ExecuteAsync(updateQuery, parameters);

                return await GetDocumentByIdAsync(documentId) ?? throw new InvalidOperationException("Document update failed.");
            }
        }

        public async Task<bool> DeleteDocumentAsync(int documentId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [Document]
                    WHERE DocumentId = @DocumentId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { DocumentId = documentId });

                if (exists == 0)
                {
                    return false;
                }

                var deleteQuery = @"
                    DELETE FROM [Document]
                    WHERE DocumentId = @DocumentId";

                await connection.ExecuteAsync(deleteQuery, new { DocumentId = documentId });

                return true;
            }
        }

        public async Task<int> CountDocumentsAsync(int? userId = null, int? farmId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder(@"
                SELECT COUNT(*)
                FROM [Document]
                WHERE 1 = 1");

                // Add conditions only if values are not null or zero
                if (userId.HasValue && userId.Value != 0)
                {
                    query.Append(" AND UserId = @UserId");
                }

                if (farmId.HasValue && farmId.Value != 0)
                {
                    query.Append(" AND FarmId = @FarmId");
                }

                // Execute the count query with the appropriate parameters
                var totalRecords = await connection.ExecuteScalarAsync<int>(query.ToString(), new
                {
                    UserId = userId,
                    FarmId = farmId
                });

                return totalRecords;
            }
        }

    }
}
