
using Application.Common.Abstractions;
using Dapper;
using Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Core.Models;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using Domain.PermitManagement.Requests;

namespace DataAccess.PermitManagement.Repositories
{
    public  class PermitRepository:IPermitRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public PermitRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<PermitApplication> CreatePermitApplication(PermitApplicationCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Insert query for creating the new application
                var insertQuery = @"
INSERT INTO [PermitApplications] (PermitId, ApplicantType, ApplicantId, Documents, Status, AppliedBy, AppliedAt,RevokedBy,AgentId)
VALUES (@PermitId, @ApplicantType, @ApplicantId, @Documents, @Status, @AppliedBy, @AppliedAt,@RevokedBy,@AgentId);
SELECT CAST(SCOPE_IDENTITY() as int);";

                // Parameters to pass into the query
                var parameters = new
                {
                    PermitId = request.PermitId, // Include PermitId from the request
                    ApplicantType = request.ApplicantType,
                    ApplicantId = request.ApplicantId,
                    Documents = request.Documents,
                    Status = "Pending", // Default status when the application is created
                    AppliedBy = request.AppliedBy,
                    AppliedAt = DateTime.UtcNow,
                    RevokedBy= "",
                    AgentId=request.AgentId
                };

                // Execute the insert query and get the new application ID
                var applicationId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                // Return the newly created application
                return new PermitApplication
                {
                    ApplicationId = applicationId,
                    PermitId = request.PermitId, // Include PermitId in the returned object
                    ApplicantType = request.ApplicantType,
                    ApplicantId = request.ApplicantId,
                    Documents = request.Documents,
                    Status = "Pending", // Default status
                    AppliedBy = request.AppliedBy,
                    AppliedAt = parameters.AppliedAt
                };
            }
        }


        public async Task<PagedResultResponse<PermitApplication>> GetAllPermitApplicationsAsync(
     int pageNumber,
     int pageSize,
     string? search = null,
     int? userId = null,
     int? permitId = null,
     string? type = null, string? agent = "no")
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Calculate how many records to skip based on the page number and page size
                var skip = (pageNumber - 1) * pageSize;

                // Base query for fetching applications
                var query = new StringBuilder(@"
SELECT ApplicationId, PermitId, ApplicantType, ApplicantId, Documents, Status, AppliedBy, AppliedAt, 
       ReviewedBy, ReviewedAt, IssuedAt, ExpiryDate, PermitPdf, RevokedAt, RevokedBy, Comments
FROM [PermitApplications]
WHERE 1=1");

                // Add filters for UserId and PermitId if they are provided
                if (userId.HasValue && userId != 0 && agent !="yes")
                {
                    query.Append(@"
                    AND AppliedBy = @UserId");
                }
                else if(userId.HasValue && userId != 0 && agent == "yes")
                {
                    query.Append(@"
                    AND AgentId = @UserId");
                }

                if (permitId.HasValue)
                {
                    query.Append(@"
    AND PermitId = @PermitId");
                }

                if (!string.IsNullOrWhiteSpace(type))
                {
                    query.Append(@"
    AND ApplicantType = @Type");
                }

                // Add search functionality for filtering by ApplicantType, Status, or Comments
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
    AND (ApplicantType LIKE @Search
    OR Status LIKE @Search
    OR Comments LIKE @Search)");
                }

                // Pagination
                query.Append(@"
ORDER BY AppliedAt DESC
OFFSET @Skip ROWS
FETCH NEXT @PageSize ROWS ONLY;

SELECT COUNT(*)
FROM [PermitApplications]
WHERE 1=1");

                // Add the same filters for UserId and PermitId to the count query
                if (userId.HasValue)
                {
                    query.Append(@"
    AND AppliedBy = @UserId");
                }

                if (permitId.HasValue)
                {
                    query.Append(@"
    AND PermitId = @PermitId");
                }

                if (!string.IsNullOrWhiteSpace(type))
                {
                    query.Append(@"
    AND ApplicantType = @Type");
                }

                // Add search conditions to the count query as well
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
    AND (ApplicantType LIKE @Search
    OR Status LIKE @Search
    OR Comments LIKE @Search)");
                }

                // Execute the query
                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    UserId = userId,
                    PermitId = permitId,
                    Type = type
                }))
                {
                    // Read the list of applications
                    var applications = multi.Read<PermitApplication>().ToList();

                    // Read the total record count
                    var totalRecords = multi.ReadSingle<int>();

                    // Return paged result
                    return new PagedResultResponse<PermitApplication>
                    {
                        Items = applications,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }


        public async Task<PermitApplication?> GetPermitApplicationByIdAsync(int applicationId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Query to get the application by ID
                var query = @"
        SELECT ApplicationId, PermitId, ApplicantType, ApplicantId, Documents, Status, AppliedBy, AppliedAt, 
               ReviewedBy, ReviewedAt, IssuedAt, ExpiryDate, PermitPdf, RevokedAt, RevokedBy, Comments
        FROM [PermitApplications]
        WHERE ApplicationId = @ApplicationId";

                // Execute the query and return the application if found, otherwise return null
                return await connection.QuerySingleOrDefaultAsync<PermitApplication>(query, new { ApplicationId = applicationId });
            }
        }


        public async Task<PermitApplication> UpdatePermitApplication(PermitApplicationUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the application exists
                var checkQuery = @"
SELECT COUNT(*)
FROM [PermitApplications]
WHERE ApplicationId = @ApplicationId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { ApplicationId = request.ApplicationId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException(request.ApplicationId);
                }

                // Initialize SQL query and parameters
                var queryBuilder = new List<string>();
                var parameters = new DynamicParameters();
                parameters.Add("ApplicationId", request.ApplicationId);

                if (request.PermitId > 0)
                {
                    queryBuilder.Add("PermitId = @PermitId");
                    parameters.Add("PermitId", request.PermitId);
                }

                if (!string.IsNullOrEmpty(request.ApplicantType))
                {
                    queryBuilder.Add("ApplicantType = @ApplicantType");
                    parameters.Add("ApplicantType", request.ApplicantType);
                }

                if (request.ApplicantId > 0)
                {
                    queryBuilder.Add("ApplicantId = @ApplicantId");
                    parameters.Add("ApplicantId", request.ApplicantId);
                }

                if (!string.IsNullOrEmpty(request.Documents))
                {
                    queryBuilder.Add("Documents = @Documents");
                    parameters.Add("Documents", request.Documents);
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    queryBuilder.Add("Status = @Status");
                    parameters.Add("Status", request.Status);
                }

                if (request.AppliedBy > 0)
                {
                    queryBuilder.Add("AppliedBy = @AppliedBy");
                    parameters.Add("AppliedBy", request.AppliedBy);
                }

                if (request.AppliedAt != DateTime.MinValue)
                {
                    queryBuilder.Add("AppliedAt = @AppliedAt");
                    parameters.Add("AppliedAt", request.AppliedAt);
                }

                if (request.ReviewedBy.HasValue)
                {
                    queryBuilder.Add("ReviewedBy = @ReviewedBy");
                    parameters.Add("ReviewedBy", request.ReviewedBy);
                }

                if (request.ReviewedAt.HasValue)
                {
                    queryBuilder.Add("ReviewedAt = @ReviewedAt");
                    parameters.Add("ReviewedAt", request.ReviewedAt);
                }

                if (request.IssuedAt.HasValue)
                {
                    queryBuilder.Add("IssuedAt = @IssuedAt");
                    parameters.Add("IssuedAt", request.IssuedAt);
                }

                if (request.ExpiryDate.HasValue)
                {
                    queryBuilder.Add("ExpiryDate = @ExpiryDate");
                    parameters.Add("ExpiryDate", request.ExpiryDate);
                }

                if (!string.IsNullOrEmpty(request.PermitPdf))
                {
                    queryBuilder.Add("PermitPdf = @PermitPdf");
                    parameters.Add("PermitPdf", request.PermitPdf);
                }

                if (request.RevokedAt.HasValue)
                {
                    queryBuilder.Add("RevokedAt = @RevokedAt");
                    parameters.Add("RevokedAt", request.RevokedAt);
                }

                if (request.RevokedBy.HasValue)
                {
                    queryBuilder.Add("RevokedBy = @RevokedBy");
                    parameters.Add("RevokedBy", request.RevokedBy);
                }

                if (!string.IsNullOrEmpty(request.Comments))
                {
                    queryBuilder.Add("Comments = @Comments");
                    parameters.Add("Comments", request.Comments);
                }

               

                if (queryBuilder.Count == 0)
                {
                    throw new ArgumentException("No fields provided to update.");
                }

                // Build the update query
                var updateQuery = $@"
UPDATE [PermitApplications]
SET {string.Join(", ", queryBuilder)}
WHERE ApplicationId = @ApplicationId";

                // Execute the update query
                await connection.ExecuteAsync(updateQuery, parameters);

                // Return the updated application details
                var updatedQuery = @"
SELECT ApplicationId, PermitId, ApplicantType, ApplicantId, Documents, Status, AppliedBy, AppliedAt, 
       ReviewedBy, ReviewedAt, IssuedAt, ExpiryDate, PermitPdf, RevokedAt, RevokedBy, Comments
FROM [PermitApplications]
WHERE ApplicationId = @ApplicationId";

                return await connection.QuerySingleAsync<PermitApplication>(updatedQuery, new { ApplicationId = request.ApplicationId });
            }
        }


        public async Task<bool> DeletePermitApplication(int applicationId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the application exists
                var checkQuery = @"
SELECT COUNT(*)
FROM [PermitApplications]
WHERE ApplicationId = @ApplicationId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { ApplicationId = applicationId });

                if (exists == 0)
                {
                    return false; // Return false if the application doesn't exist
                }

                // Prepare the delete query
                var deleteQuery = @"
DELETE FROM [PermitApplications]
WHERE ApplicationId = @ApplicationId";

                // Execute the delete query
                await connection.ExecuteAsync(deleteQuery, new { ApplicationId = applicationId });

                return true; // Return true indicating the application was deleted successfully
            }
        }

        public async Task<int> CountPermitApplicationsAsync(int? applicantId = null, int? applicantType = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder(@"
SELECT COUNT(*)
FROM [PermitApplications]
WHERE 1 = 1");

                // Add conditions only if values are not null or zero
                if (applicantId.HasValue && applicantId.Value != 0)
                {
                    query.Append(" AND ApplicantId = @ApplicantId");
                }

                if (applicantType.HasValue && applicantType.Value != 0)
                {
                    query.Append("AND applicantType = @ApplicantType");
                }

                // Execute the count query with the appropriate parameters
                var totalRecords = await connection.ExecuteScalarAsync<int>(query.ToString(), new
                {
                    ApplicantId = applicantId,
                    ApplicantType = applicantType
                });

                return totalRecords;
            }
        }

        public async Task<int> CountPendingApplicationsAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
        SELECT COUNT(*)
        FROM [PermitApplications]
        WHERE Status = @Status";

                // Execute the count query with the "Pending" status
                var totalRecords = await connection.ExecuteScalarAsync<int>(query, new
                {
                    Status = "Pending"
                });

                return totalRecords;
            }
        }


        public async Task<int> CreatePermitAsync(PermitCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
        INSERT INTO [Permits] (PermitName, Description, Requirements, CreatedAt, UpdatedAt)
        VALUES (@PermitName, @Description, @Requirements, @CreatedAt, @UpdatedAt);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var parameters = new
                {
                    request.PermitName,
                    request.Description,
                    request.Requirements,
                    request.CreatedAt,
                    UpdatedAt = request.UpdatedAt // Nullable so we can pass null if not updated
                };

                var permitId = await connection.QuerySingleAsync<int>(query, parameters);
                return permitId;
            }
        }


        public async Task<Permit?> GetPermitByIdAsync(int permitId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
        SELECT PermitId, PermitName, Description, Requirements, CreatedAt, UpdatedAt
        FROM [Permits]
        WHERE PermitId = @PermitId";

                return await connection.QuerySingleOrDefaultAsync<Permit>(query, new { PermitId = permitId });
            }
        }

        public async Task<Permit> UpdatePermitAsync(PermitUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the permit exists
                var checkQuery = @"
        SELECT COUNT(*)
        FROM [Permits]
        WHERE PermitId = @PermitId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { PermitId = request.PermitId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException(request.PermitId);
                }

                // Initialize SQL query and parameters
                var queryBuilder = new List<string>();
                var parameters = new DynamicParameters();
                parameters.Add("PermitId", request.PermitId);

                if (!string.IsNullOrEmpty(request.PermitName))
                {
                    queryBuilder.Add("PermitName = @PermitName");
                    parameters.Add("PermitName", request.PermitName);
                }

                if (!string.IsNullOrEmpty(request.Description))
                {
                    queryBuilder.Add("Description = @Description");
                    parameters.Add("Description", request.Description);
                }

                if (!string.IsNullOrEmpty(request.Requirements))
                {
                    queryBuilder.Add("Requirements = @Requirements");
                    parameters.Add("Requirements", request.Requirements);
                }

                if (request.UpdatedAt.HasValue)
                {
                    queryBuilder.Add("UpdatedAt = @UpdatedAt");
                    parameters.Add("UpdatedAt", request.UpdatedAt);
                }

                if (queryBuilder.Count == 0)
                {
                    throw new ArgumentException("No fields provided to update.");
                }

                var updateQuery = $@"
        UPDATE [Permits]
        SET {string.Join(", ", queryBuilder)}
        WHERE PermitId = @PermitId";

                // Execute the update query
                await connection.ExecuteAsync(updateQuery, parameters);

                // Return the updated permit details
                var updatedQuery = @"
        SELECT PermitId, PermitName, Description, Requirements, CreatedAt, UpdatedAt
        FROM [Permits]
        WHERE PermitId = @PermitId";

                return await connection.QuerySingleAsync<Permit>(updatedQuery, new { PermitId = request.PermitId });
            }
        }

        public async Task<bool> DeletePermitAsync(int permitId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the permit exists
                var checkQuery = @"
        SELECT COUNT(*)
        FROM [Permits]
        WHERE PermitId = @PermitId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { PermitId = permitId });

                if (exists == 0)
                {
                    return false; // Return false if the permit doesn't exist
                }

                // Prepare the delete query
                var deleteQuery = @"
        DELETE FROM [Permits]
        WHERE PermitId = @PermitId";

                // Execute the delete query
                await connection.ExecuteAsync(deleteQuery, new { PermitId = permitId });

                return true; // Return true indicating the permit was deleted successfully
            }
        }

        public async Task<int> CountPermitsAsync(string? permitName = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder(@"
        SELECT COUNT(*)
        FROM [Permits]
        WHERE 1 = 1");

                // Add filtering conditions
                if (!string.IsNullOrEmpty(permitName))
                {
                    query.Append(" AND PermitName LIKE @PermitName");
                }

                // Execute the count query with parameters
                var totalRecords = await connection.ExecuteScalarAsync<int>(query.ToString(), new
                {
                    PermitName = permitName != null ? "%" + permitName + "%" : null
                });

                return totalRecords;
            }
        }

        public async Task<PagedResultResponse<Permit>> GetAllPermitsAsync(
    int pageNumber,
    int pageSize,
    string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
        SELECT PermitId, PermitName, Description, Requirements, CreatedAt, UpdatedAt
        FROM [Permits]
        WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
        AND (PermitName LIKE @Search
        OR Description LIKE @Search
        OR Requirements LIKE @Search)");
                }

                query.Append(@"
        ORDER BY CreatedAt DESC
        OFFSET @Skip ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(*)
        FROM [Permits]
        WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
        AND (PermitName LIKE @Search
        OR Description LIKE @Search
        OR Requirements LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var permits = multi.Read<Permit>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Permit>
                    {
                        Items = permits,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }








    }
}
