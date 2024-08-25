using Application.Common.Abstractions;
using Application.FarmManagement.Abstractions;
using Application.LivestockManagement.Abstractions;
using Dapper;
using DataAccess.Common.Exceptions;
using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.FarmManagement.Requests;
using Domain.FarmManagement.Responses;
using Domain.LivestockManagement.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.LivestockManagement.Repositories
{
    public class LivestockManagementRepository : ILivestockManagementRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public LivestockManagementRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<Livestock> CreateLivestock(LivestockCreationRequest request, int userId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the FarmId exists
                var farmExistsQuery = @"
                SELECT COUNT(*)
                FROM [Farm]
                WHERE FarmId = @FarmId AND UserId = @UserId";

                var farmExists = await connection.QuerySingleAsync<int>(farmExistsQuery, new
                {
                    FarmId = request.FarmId,
                    UserId = userId
                });

                if (farmExists == 0)
                {
                    throw new ItemDoesNotExistException($"The specified Farm ID: {request.FarmId} does not exist.");
                }

                // Check if the Livestock with the same IdentificationMark already exists
                var livestockExistsQuery = @"
        SELECT COUNT(*)
        FROM [Livestock]
        WHERE IdentificationMark = @IdentificationMark AND UserId = @UserId";

                var existingCount = await connection.QuerySingleAsync<int>(livestockExistsQuery, new
                {
                    IdentificationMark = request.IdentificationMark,
                    UserId = userId
                });

                if (existingCount > 0)
                {
                    throw new ItemAlreadyExistsException("Livestock with this identification mark already exists for this user.");
                }

                // Insert new Livestock record
                var insertQuery = @"
                INSERT INTO [Livestock] (UserId, FarmId, Species, Breed, DateOfBirth, HealthStatus, IdentificationMark, CreatedAt, UpdatedAt)
                VALUES (@UserId, @FarmId, @Species, @Breed, @DateOfBirth, @HealthStatus, @IdentificationMark, @CreatedAt, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    UserId = userId,
                    FarmId = request.FarmId,
                    Species = request.Species,
                    Breed = request.Breed,
                    DateOfBirth = request.DateOfBirth,
                    HealthStatus = request.HealthStatus,
                    IdentificationMark = request.IdentificationMark,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var livestockId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new Livestock
                {
                    LivestockId = livestockId,
                    UserId = userId,
                    FarmId = request.FarmId,
                    Species = request.Species,
                    Breed = request.Breed,
                    DateOfBirth = request.DateOfBirth,
                    HealthStatus = request.HealthStatus,
                    IdentificationMark = request.IdentificationMark,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt
                };
            }
        }


        public async Task<PagedResultResponse<Livestock>> GetLivestockByFarmAsync(int farmId, int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                SELECT *
                FROM [Livestock]
                WHERE FarmId = @FarmId");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Species LIKE @Search
                OR Breed LIKE @Search
                OR IdentificationMark LIKE @Search)");
                }

                query.Append(@"
                ORDER BY IdentificationMark
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM [Livestock]
                WHERE FarmId = @FarmId");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Species LIKE @Search
                OR Breed LIKE @Search
                OR IdentificationMark LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    FarmId = farmId,
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var livestock = multi.Read<Livestock>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Livestock>
                    {
                        Items = livestock,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Livestock?> GetLivestockByIdAsync(int livestockId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [Livestock]
                WHERE LivestockId = @LivestockId";

                var livestock = await connection.QuerySingleOrDefaultAsync<Livestock>(query, new
                {
                    LivestockId = livestockId
                });

                return livestock;
            }
        }

        public async Task<Livestock?> UpdateLivestockAsync(LivestockUpdateRequest request, int userId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var checkQuery = @"
                SELECT COUNT(*)
                FROM [Livestock]
                WHERE LivestockId = @LivestockId AND UserId = @UserId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new
                {
                    LivestockId = request.LivestockId,
                    UserId = userId
                });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException("Livestock does not exist.");
                }

                // Update the livestock record
                var updateQuery = @"
                UPDATE [Livestock]
                SET FarmId = @FarmId,
                    Species = @Species,
                    Breed = @Breed,
                    DateOfBirth = @DateOfBirth,
                    HealthStatus = @HealthStatus,
                    IdentificationMark = @IdentificationMark,
                    UpdatedAt = @UpdatedAt
                WHERE LivestockId = @LivestockId AND UserId = @UserId";

                var parameters = new
                {
                    request.FarmId,
                    request.Species,
                    request.Breed,
                    request.DateOfBirth,
                    request.HealthStatus,
                    request.IdentificationMark,
                    request.LivestockId,
                    UpdatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                await connection.ExecuteAsync(updateQuery, parameters);

                
                var livestock = await GetLivestockByIdAsync(request.LivestockId);

                return livestock;
            }
        }

        public async Task<bool> DeleteLivestockAsync(int livestockId, int userId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var checkQuery = @"
                SELECT COUNT(*)
                FROM [Livestock]
                WHERE LivestockId = @LivestockId AND UserId = @UserId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new
                {
                    LivestockId = livestockId,
                    UserId = userId
                });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException("Livestock does not exist.");
                }

                
                var deleteQuery = @"
                DELETE FROM [Livestock]
                WHERE LivestockId = @LivestockId AND UserId = @UserId";

                var rowsAffected = await connection.ExecuteAsync(deleteQuery, new
                {
                    LivestockId = livestockId,
                    UserId = userId
                });

                return rowsAffected > 0;
            }
        }

        public async Task<HealthRecord> CreateHealthRecordAsync(HealthRecordCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var livestockExistsQuery = @"
                SELECT COUNT(*)
                FROM [Livestock]
                WHERE LivestockId = @LivestockId AND UserId = @UserId";

                var livestockExists = await connection.QuerySingleAsync<int>(livestockExistsQuery, new
                {
                    LivestockId = request.LivestockId,
                    UserId = request.UserId // Use the UserId from the request
                });

                if (livestockExists == 0)
                {
                    throw new ItemDoesNotExistException($"The specified Livestock ID: {request.LivestockId} does not exist.");
                }

               
                var insertQuery = @"
                INSERT INTO [HealthRecord] (LivestockId, UserId, DateOfVisit, Diagnosis, Treatment, FollowUpDate, CreatedAt, UpdatedAt)
                VALUES (@LivestockId, @UserId, @DateOfVisit, @Diagnosis, @Treatment, @FollowUpDate, @CreatedAt, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    LivestockId = request.LivestockId,
                    UserId = request.UserId,
                    DateOfVisit = request.DateOfVisit,
                    Diagnosis = request.Diagnosis,
                    Treatment = request.Treatment,
                    FollowUpDate = request.FollowUpDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var healthRecordId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new HealthRecord
                {
                    HealthRecordId = healthRecordId,
                    LivestockId = request.LivestockId,
                    UserId = request.UserId, 
                    DateOfVisit = request.DateOfVisit,
                    Diagnosis = request.Diagnosis,
                    Treatment = request.Treatment,
                    FollowUpDate = request.FollowUpDate,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt
                };
            }
        }


        public async Task<PagedResultResponse<HealthRecord>> GetHealthRecordsByLivestockIdAsync(int livestockId, int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                SELECT *
                FROM [HealthRecord]
                WHERE LivestockId = @LivestockId");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Diagnosis LIKE @Search
                OR Treatment LIKE @Search)");
                }

                query.Append(@"
                ORDER BY DateOfVisit
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM [HealthRecord]
                WHERE LivestockId = @LivestockId");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Diagnosis LIKE @Search
                OR Treatment LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    LivestockId = livestockId,
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var healthRecords = multi.Read<HealthRecord>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<HealthRecord>
                    {
                        Items = healthRecords,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }


        public async Task<HealthRecord?> GetHealthRecordByIdAsync(int healthRecordId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [HealthRecord]
                WHERE RecordId = @HealthRecordId";

                return await connection.QueryFirstOrDefaultAsync<HealthRecord>(query, new { HealthRecordId = healthRecordId });
            }
        }

        public async Task<bool> UpdateHealthRecordAsync(int healthRecordId, UpdateHealthRecordRequest updateRequest)
{
    using (var connection = _dbConnectionProvider.CreateConnection())
    {
        connection.Open();

        var query = new StringBuilder("UPDATE [HealthRecord] SET ");
        var parameters = new DynamicParameters();
        parameters.Add("RecordId", healthRecordId);

        if (updateRequest.LivestockId.HasValue)
        {
            query.Append("LivestockId = @LivestockId, ");
            parameters.Add("LivestockId", updateRequest.LivestockId);
        }

        if (!string.IsNullOrWhiteSpace(updateRequest.Diagnosis))
        {
            query.Append("Diagnosis = @Diagnosis, ");
            parameters.Add("Diagnosis", updateRequest.Diagnosis);
        }

        if (!string.IsNullOrWhiteSpace(updateRequest.Treatment))
        {
            query.Append("Treatment = @Treatment, ");
            parameters.Add("Treatment", updateRequest.Treatment);
        }

        if (updateRequest.FollowUpDate.HasValue)
        {
            query.Append("FollowUpDate = @FollowUpDate, ");
            parameters.Add("FollowUpDate", updateRequest.FollowUpDate);
        }

        if (updateRequest.DateOfVisit.HasValue)
        {
            query.Append("DateOfVisit = @DateOfVisit, ");
            parameters.Add("DateOfVisit", updateRequest.DateOfVisit);
        }

        
        query.Append("UpdatedAt = @UpdatedAt ");
        parameters.Add("UpdatedAt", DateTime.UtcNow);

        query.Append("WHERE RecordId = @RecordId");

        var result = await connection.ExecuteAsync(query.ToString(), parameters);

        return result > 0;
    }
}


        public async Task<bool> DeleteHealthRecordAsync(int healthRecordId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                DELETE FROM [HealthRecord]
                WHERE RecordId = @HealthRecordId";

                var result = await connection.ExecuteAsync(query, new { HealthRecordId = healthRecordId });

                return result > 0;
            }
        }

        public async Task<int> CreateDirectiveAsync(CreateDirectiveRequest newDirective)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var livestockExistsQuery = @"
                SELECT COUNT(*)
                FROM [Livestock]
                WHERE LivestockId = @LivestockId";

                var livestockExists = await connection.ExecuteScalarAsync<int>(livestockExistsQuery, new
                {
                    LivestockId = newDirective.LivestockId
                });

                if (livestockExists == 0)
                {
                    throw new ItemDoesNotExistException($"The livestock with id:{newDirective.LivestockId} does not exist");
                }

                
                var query = @"
                INSERT INTO [Directive] (LivestockId, DirectiveDate, DirectiveDetails, CreatedAt, UpdatedAt)
                VALUES (@LivestockId, @DirectiveDate, @DirectiveDetails, @CreatedAt, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var directiveId = await connection.QuerySingleAsync<int>(query, new
                {
                    LivestockId = newDirective.LivestockId,
                    DirectiveDate = newDirective.DirectiveDate,
                    DirectiveDetails = newDirective.DirectiveDetails,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                return directiveId;
            }
        }

        public async Task<PagedResultResponse<Directive>> GetDirectivesByLivestockAsync(int livestockId, int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
        SELECT *
        FROM [Directive]
        WHERE LivestockId = @LivestockId");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
            AND DirectiveDetails LIKE @Search");
                }

                query.Append(@"
        ORDER BY DirectiveDate
        OFFSET @Skip ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(*)
        FROM [Directive]
        WHERE LivestockId = @LivestockId");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
            AND DirectiveDetails LIKE @Search");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    LivestockId = livestockId,
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var directives = multi.Read<Directive>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Directive>
                    {
                        Items = directives,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Directive?> GetDirectiveByIdAsync(int directiveId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [Directive]
                WHERE DirectiveId = @DirectiveId";

                return await connection.QueryFirstOrDefaultAsync<Directive>(query, new { DirectiveId = directiveId });
            }
        }

        public async Task<bool> UpdateDirectiveDetailsAsync(int directiveId, string directiveDetails)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                UPDATE [Directive]
                SET DirectiveDetails = @DirectiveDetails,
                    UpdatedAt = @UpdatedAt
                WHERE DirectiveId = @DirectiveId";

                var result = await connection.ExecuteAsync(query, new
                {
                    DirectiveId = directiveId,
                    DirectiveDetails = directiveDetails,
                    UpdatedAt = DateTime.UtcNow
                });

                return result > 0; 
            }
        }

        public async Task<bool> DeleteDirectiveAsync(int directiveId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                DELETE FROM [Directive]
                WHERE DirectiveId = @DirectiveId";

                var result = await connection.ExecuteAsync(query, new { DirectiveId = directiveId });

                return result > 0;
            }
        }









    }
}
