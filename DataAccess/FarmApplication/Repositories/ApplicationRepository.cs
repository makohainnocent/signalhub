using Application.Application.Abstractions;
using Application.Common.Abstractions;
using Dapper;
using Domain.FarmApplication.Requests;
using Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Core.Models;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;

namespace DataAccess.FarmApplication.Repositories
{
    public  class ApplicationRepository:IApplicationRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public ApplicationRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<FarmApplicationModel> CreateApplication(ApplicationCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // If you need to check for existing applications, you can add a similar query as in CreateFarm
                // For now, we assume no duplicate check is required

                // Insert query for creating the new application
                var insertQuery = @"
        INSERT INTO [Application] (FarmId, UserId, Type, RequestObject,RequestDescription, Status, CreatedAt, UpdatedAt)
        VALUES (@FarmId, @UserId, @Type, @RequestObject,@RequestDescription, @Status, @CreatedAt, @UpdatedAt);
        SELECT CAST(SCOPE_IDENTITY() as int);";

                // Parameters to pass into the query
                var parameters = new
                {
                    FarmId = request.FarmId,
                    UserId = request.UserId,
                    Type = request.Type,
                    RequestObject = request.RequestObject,
                    RequestDescription = request.RequestDescription,
                    Status = "Pending", // Default status when the application is created
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Execute the insert query and get the new application ID
                var applicationId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                // Return the newly created application
                return new FarmApplicationModel
                {
                    ApplicationId = applicationId,
                    FarmId = request.FarmId,
                    UserId = request.UserId,
                    Type = request.Type,
                    RequestObject = request.RequestObject,
                    RequestDescription= request.RequestDescription,
                    Status = parameters.Status,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt
                };
            }
        }

        public async Task<PagedResultResponse<FarmApplicationModel>> GetAllApplicationsAsync(
    int pageNumber,
    int pageSize,
    string? search = null,
    int? userId = null,
    int? farmId = null,
    string? type = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Calculate how many records to skip based on the page number and page size
                var skip = (pageNumber - 1) * pageSize;

                // Base query for fetching applications
                var query = new StringBuilder(@"
SELECT *
FROM [Application]
WHERE 1=1");

                // Add filters for UserId and FarmId if they are provided
                if (userId.HasValue && userId!=0)
                {
                    query.Append(@"
    AND UserId = @UserId");
                }

                if (farmId.HasValue)
                {
                    query.Append(@"
    AND FarmId = @FarmId");
                }

                if (!string.IsNullOrWhiteSpace(type))
                {
                    query.Append(@"
    AND Type = @Type");
                }

                // Add search functionality for filtering by Type or Status
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
    AND (Type LIKE @Search
    OR Status LIKE @Search)");
                }

                // Pagination
                query.Append(@"
ORDER BY CreatedAt DESC
OFFSET @Skip ROWS
FETCH NEXT @PageSize ROWS ONLY;

SELECT COUNT(*)
FROM [Application]
WHERE 1=1");

                // Add the same filters for UserId and FarmId to the count query
                if (userId.HasValue)
                {
                    query.Append(@"
    AND UserId = @UserId");
                }

                if (farmId.HasValue)
                {
                    query.Append(@"
    AND FarmId = @FarmId");
                }

                if (!string.IsNullOrWhiteSpace(type))
                {
                    query.Append(@"
    AND Type = @Type");
                }

                // Add search conditions to the count query as well
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
    AND (Type LIKE @Search
    OR Status LIKE @Search)");
                }

                // Execute the query
                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    UserId = userId,
                    FarmId = farmId,
                    Type = type
                }))
                {
                    // Read the list of applications
                    var applications = multi.Read<FarmApplicationModel>().ToList();

                    // Read the total record count
                    var totalRecords = multi.ReadSingle<int>();

                    // Return paged result
                    return new PagedResultResponse<FarmApplicationModel>
                    {
                        Items = applications,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }


        public async Task<FarmApplicationModel?> GetApplicationByIdAsync(int applicationId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Query to get the application by ID
                var query = @"
                SELECT *
                FROM [Application]
                WHERE ApplicationId = @ApplicationId";

                // Execute the query and return the application if found, otherwise return null
                return await connection.QuerySingleOrDefaultAsync<FarmApplicationModel>(query, new { ApplicationId = applicationId });
            }
        }

        public async Task<FarmApplicationModel> UpdateApplication(ApplicationUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the application exists
                var checkQuery = @"
        SELECT COUNT(*)
        FROM [Application]
        WHERE ApplicationId = @ApplicationId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { ApplicationId = request.ApplicationId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException(request.ApplicationId);
                }

                // Prepare the SQL query to update the application
                var updateQuery = @"
        UPDATE [Application]
        SET FarmId = @FarmId,
            UserId = @UserId,
            Type = @Type,
            RequestObject = @RequestObject,
            RequestDescription=@RequestDescription,
            Status = @Status,
            ResponseObject = @ResponseObject,
            ResponseDescription=@ResponseDescription,
            UpdatedAt = @UpdatedAt
        WHERE ApplicationId = @ApplicationId";

                // Prepare the parameters
                var parameters = new
                {
                    ApplicationId = request.ApplicationId,
                    FarmId = request.FarmId,
                    UserId = request.UserId,
                    Type = request.Type,
                    RequestObject = request.RequestObject,
                    RequestDescription = request.RequestDescription,
                    Status = request.Status ?? "Pending",  // Keep default if null
                    ResponseObject = request.ResponseObject,
                    ResponseDescription = request.ResponseDescription,
                    UpdatedAt = DateTime.UtcNow
                };

                // Execute the update query
                await connection.ExecuteAsync(updateQuery, parameters);

                // Return the updated application details
                return new FarmApplicationModel
                {
                    ApplicationId = request.ApplicationId,
                    FarmId = request.FarmId,
                    UserId = request.UserId,
                    Type = request.Type,
                    RequestObject = request.RequestObject,
                    Status = parameters.Status,
                    ResponseObject = request.ResponseObject,
                    UpdatedAt = parameters.UpdatedAt,
                    CreatedAt = request.CreatedAt // Assume this doesn't change during the update
                };
            }
        }

        public async Task<bool> DeleteApplication(int applicationId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the application exists
                var checkQuery = @"
        SELECT COUNT(*)
        FROM [Application]
        WHERE ApplicationId = @ApplicationId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { ApplicationId = applicationId });

                if (exists == 0)
                {
                    return false; // Return false if the application doesn't exist
                }

                // Prepare the delete query
                var deleteQuery = @"
        DELETE FROM [Application]
        WHERE ApplicationId = @ApplicationId";

                // Execute the delete query
                await connection.ExecuteAsync(deleteQuery, new { ApplicationId = applicationId });

                return true; // Return true indicating the application was deleted successfully
            }
        }

        public async Task<int> CountApplicationsAsync(int? userId = null, int? farmId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder(@"
        SELECT COUNT(*)
        FROM [Application]
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
