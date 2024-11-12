using Dapper;
using Domain.InspectionManagement.Requests;
using Domain.Core.Models;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Abstractions;
using Application.InspectionManagement.Abstractions;

namespace DataAccess.InspectionManagement.Repositories
{
    public class InspectionRepository : IInspectionRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public InspectionRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<Inspection> CreateInspectionAsync(CreateInspectionRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                INSERT INTO [Inspection] (UserId, EntityIds,EntityType, InspectionDate, Outcome, Notes, CreatedAt, UpdatedAt)
                VALUES (@UserId, @EntityIds,@EntityType, @InspectionDate, @Outcome, @Notes, @CreatedAt, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    request.UserId,
                    request.EntityIds,
                    request.EntityType,
                    request.InspectionDate,
                    request.Outcome,
                    request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var inspectionId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new Inspection
                {
                    InspectionId = inspectionId,
                    UserId = request.UserId,
                    EntityIds = request.EntityIds,
                    EntityType = request.EntityType,
                    InspectionDate = request.InspectionDate,
                    Outcome = request.Outcome,
                    Notes = request.Notes,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt
                };
            }
        }

        public async Task<Inspection?> GetInspectionByIdAsync(int inspectionId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [Inspection]
                WHERE InspectionId = @InspectionId";

                return await connection.QuerySingleOrDefaultAsync<Inspection>(query, new { InspectionId = inspectionId });
            }
        }

        public async Task<PagedResultResponse<Inspection>> GetAllInspectionsAsync(int pageNumber, int pageSize, string? search = null, int? userId = 0)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
        SELECT *
        FROM [Inspection]
        WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    // Search in Outcome and Notes fields
                    query.Append(" AND (Outcome LIKE @Search OR Notes LIKE @Search)");
                }

                if (userId.HasValue&userId!=0)
                {
                    query.Append(" AND (UserId = @UserId)");
                }

                query.Append(@"
        ORDER BY InspectionDate DESC
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(*)
        FROM [Inspection]
        WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(" AND (Outcome LIKE @Search OR Notes LIKE @Search)");
                }

                if (userId.HasValue & userId != 0)
                {
                    query.Append(" AND (UserId = @UserId)");
                }

                try
                {
                    using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                    {
                        Search = $"%{search}%",
                        Skip = skip,
                        PageSize = pageSize,
                        UserId = userId
                    }))
                    {
                        var inspections = multi.Read<Inspection>().ToList();
                        var totalRecords = multi.ReadSingle<int>();

                        return new PagedResultResponse<Inspection>
                        {
                            Items = inspections,
                            TotalCount = totalRecords,
                            PageNumber = pageNumber,
                            PageSize = pageSize
                        };
                    }
                }
                catch (Exception ex)
                {
                    // Log and handle the exception appropriately
                    throw new Exception("An error occurred while retrieving inspections.", ex);
                }
            }
        }

        public async Task<Inspection> UpdateInspectionAsync(Inspection inspection)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                UPDATE [Inspection]
                SET UserId = @UserId,
                    EntityIds = @EntityIds,
                    InspectionDate = @InspectionDate,
                    Outcome = @Outcome,
                    Notes = @Notes,
                    UpdatedAt = @UpdatedAt
                WHERE InspectionId = @InspectionId";

                var parameters = new
                {
                    inspection.InspectionId,
                    inspection.UserId,
                    inspection.EntityIds,
                    inspection.InspectionDate,
                    inspection.Outcome,
                    inspection.Notes,
                    UpdatedAt = DateTime.UtcNow
                };

                await connection.ExecuteAsync(updateQuery, parameters);
                inspection.UpdatedAt = parameters.UpdatedAt;
                return inspection;
            }
        }

        public async Task<bool> DeleteInspectionAsync(int inspectionId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var deleteQuery = @"
                DELETE FROM [Inspection]
                WHERE InspectionId = @InspectionId";

                var affectedRows = await connection.ExecuteAsync(deleteQuery, new { InspectionId = inspectionId });

                return affectedRows > 0;
            }
        }

        public async Task<int> CountInspectionsAsync(int? userId = null, int? livestockId = null, int? farmId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder(@"
SELECT COUNT(*)
FROM [Inspection] i
WHERE 1 = 1");

                // Add conditions for userId
                if (userId.HasValue && userId.Value != 0)
                {
                    query.Append(" AND i.UserId = @UserId");
                }

                // Add conditions based on EntityType and EntityIds
                if (livestockId.HasValue && livestockId.Value != 0)
                {
                    query.Append(@"
            AND i.EntityType = 'Livestock'
            AND EXISTS (
                SELECT 1 FROM STRING_SPLIT(i.EntityIds, ',') AS entityId
                WHERE entityId.value = @LivestockId
            )");
                }

                if (farmId.HasValue && farmId.Value != 0)
                {
                    query.Append(@"
            AND i.EntityType = 'Farm Premises'
            AND EXISTS (
                SELECT 1 FROM STRING_SPLIT(i.EntityIds, ',') AS entityId
                WHERE entityId.value = @FarmId
            )");
                }

                // Execute the count query with the appropriate parameters
                var totalRecords = await connection.ExecuteScalarAsync<int>(query.ToString(), new
                {
                    UserId = userId,
                    LivestockId = livestockId?.ToString(),
                    FarmId = farmId?.ToString()
                });

                return totalRecords;
            }
        }

    }
}
