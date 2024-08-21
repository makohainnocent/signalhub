using Domain.Core.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Abstractions;
using Application.Authentication.Abstractions;
using Domain.Authentication.Requests;
using System.Security.Cryptography;
using System.Text;
using DataAccess.Common.Utilities;
using System.Security.Authentication;
using DataAccess.Authentication.Exceptions;
using System.Transactions;
using Domain.Common.Responses;
using Domain.Authentication.Responses;
using DataAccess.Common.Exceptions;
using Application.FarmManagement.Abstractions;
using Domain.FarmManagement.Requests;
using Domain.FarmManagement.Responses;


namespace DataAccess.FarmManagement.Repositories
{
    public class FarmManagementRepository : IFarmManagementRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public FarmManagementRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }


        public async Task<Farm> CreateFarm(FarmCreationRequest request, int userId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var checkQuery = @"
                SELECT COUNT(*)
                FROM [Farm]
                WHERE FarmName = @FarmName AND UserId = @UserId";

                var existingCount = await connection.QuerySingleAsync<int>(checkQuery, new
                {
                    FarmName = request.FarmName,
                    UserId = userId
                });

                if (existingCount > 0)
                {
                    throw new ItemAlreadyExistsException("Farm name already exists for this user.");
                }

                
                var insertQuery = @"
                INSERT INTO [Farm] (UserId, FarmName, Location, Area, CreatedAt, UpdatedAt)
                VALUES (@UserId, @FarmName, @Location, @Area, @CreatedAt, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                
                var parameters = new
                {
                    UserId = userId,
                    FarmName = request.FarmName,
                    Location = request.Location,
                    Area = request.Area,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                
                var farmId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                
                return new Farm
                {
                    FarmId = farmId,
                    UserId = userId,
                    FarmName = request.FarmName,
                    Location = request.Location,
                    Area = request.Area,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt
                };
            }
        }

        public async Task<PagedResultResponse<Farm>> GetAllFarmsAsync(int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var skip = (pageNumber - 1) * pageSize;

                
                var query = new StringBuilder(@"
            SELECT *
            FROM [Farm]
            WHERE 1=1");

                
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (FarmName LIKE @Search
                OR Location LIKE @Search)");
                }

             
                query.Append(@"
            ORDER BY FarmName
            OFFSET @Skip ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        
            SELECT COUNT(*)
            FROM [Farm]
            WHERE 1=1");

                
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (FarmName LIKE @Search
                OR Location LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var farms = multi.Read<Farm>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Farm>
                    {
                        Items = farms,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Farm?> GetFarmByIdAsync(int farmId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
            SELECT *
            FROM [Farm]
            WHERE FarmId = @FarmId";

                return await connection.QuerySingleOrDefaultAsync<Farm>(query, new { FarmId = farmId });
            }
        }

        public async Task<Farm> UpdateFarm(FarmUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var checkQuery = @"
            SELECT COUNT(*)
            FROM [Farm]
            WHERE FarmId = @FarmId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { FarmId = request.FarmId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException(request.FarmId);
                }

                // Prepare the SQL query to update the farm
                var updateQuery = @"
            UPDATE [Farm]
            SET FarmName = @FarmName,
                Location = @Location,
                Area = @Area,
                UpdatedAt = @UpdatedAt
            WHERE FarmId = @FarmId";

                // Prepare the parameters
                var parameters = new
                {
                    FarmId = request.FarmId,
                    FarmName = request.FarmName,
                    Location = request.Location,
                    Area = request.Area,
                    UpdatedAt = DateTime.UtcNow
                };

                // Execute the update
                await connection.ExecuteAsync(updateQuery, parameters);

                // Return the updated farm details
                return new Farm
                {
                    FarmId = request.FarmId,
                    FarmName = request.FarmName,
                    Location = request.Location,
                    Area = request.Area,
                    UpdatedAt = parameters.UpdatedAt
                };
            }
        }

        public async Task<bool> DeleteFarm(int farmId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var checkQuery = @"
            SELECT COUNT(*)
            FROM [Farm]
            WHERE FarmId = @FarmId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { FarmId = farmId });

                if (exists == 0)
                {
                    return false; 
                }

                
                var deleteQuery = @"
            DELETE FROM [Farm]
            WHERE FarmId = @FarmId";

                
                await connection.ExecuteAsync(deleteQuery, new { FarmId = farmId });

                return true; 
            }
        }

        public async Task<FarmGeofencing> CreateFarmGeofencing(FarmGeofencingRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var checkQuery = @"
                SELECT COUNT(*)
                FROM [Farm]
                WHERE FarmId = @FarmId";

                var farmExists = await connection.QuerySingleAsync<int>(checkQuery, new { FarmId = request.FarmId });

                if (farmExists == 0)
                {
                    throw new ItemDoesNotExistException($"Farm with ID {request.FarmId} does not exist.");
                }

                
                var insertQuery = @"
                INSERT INTO [FarmGeofencing] (FarmId, Latitude, Longitude, Radius, CreatedAt, UpdatedAt)
                VALUES (@FarmId, @Latitude, @Longitude, @Radius, @CreatedAt, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                
                var parameters = new
                {
                    FarmId = request.FarmId,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Radius = request.Radius,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                
                var geofencingId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                
                return new FarmGeofencing
                {
                    GeofenceId = geofencingId,
                    FarmId = request.FarmId,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Radius = request.Radius,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt
                };
            }
        }

        public async Task<PagedResultResponse<FarmGeofencingWithFarmDetailsResponse>> GetAllFarmGeofencingsAsync(int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
            SELECT fg.GeofenceId, fg.FarmId, fg.Latitude, fg.Longitude, fg.Radius, fg.CreatedAt, fg.UpdatedAt,
                   f.FarmName, f.Location
            FROM FarmGeofencing fg
            INNER JOIN Farm f ON fg.FarmId = f.FarmId
            WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (f.FarmName LIKE @Search
                OR f.Location LIKE @Search)");
                }

                query.Append(@"
            ORDER BY f.FarmName
            OFFSET @Skip ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM FarmGeofencing fg
            INNER JOIN Farm f ON fg.FarmId = f.FarmId
            WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (f.FarmName LIKE @Search
                OR f.Location LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var geofencings = multi.Read<FarmGeofencingWithFarmDetailsResponse>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<FarmGeofencingWithFarmDetailsResponse>
                    {
                        Items = geofencings,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<FarmGeofencingWithFarmDetailsResponse?> GetMostRecentGeofenceByFarmIdAsync(int farmId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
            SELECT TOP 1
                   fg.GeofenceId, fg.FarmId, fg.Latitude, fg.Longitude, fg.Radius, fg.CreatedAt, fg.UpdatedAt,
                   f.FarmName, f.Location
            FROM FarmGeofencing fg
            INNER JOIN Farm f ON fg.FarmId = f.FarmId
            WHERE fg.FarmId = @FarmId
            ORDER BY fg.UpdatedAt DESC";

                var geofence = await connection.QuerySingleOrDefaultAsync<FarmGeofencingWithFarmDetailsResponse>(query, new
                {
                    FarmId = farmId
                });

                return geofence;
            }
        }

        public async Task<FarmGeofencing> UpdateGeofenceAsync(FarmGeofencingUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

               
                var checkQuery = @"
                SELECT COUNT(*)
                FROM FarmGeofencing
                WHERE GeofenceId = @GeofenceId";

                var exists = await connection.ExecuteScalarAsync<bool>(checkQuery, new
                {
                    GeofenceId = request.GeofenceId
                });

                if (!exists)
                {
                    throw new ItemDoesNotExistException(request.GeofenceId);
                }

                var updateQuery = @"
                UPDATE FarmGeofencing
                SET Latitude = @Latitude,
                Longitude = @Longitude,
                Radius = @Radius,
                UpdatedAt = @UpdatedAt
                WHERE GeofenceId = @GeofenceId";

                var rowsAffected = await connection.ExecuteAsync(updateQuery, new
                {
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Radius = request.Radius,
                    UpdatedAt = DateTime.UtcNow,
                    GeofenceId = request.GeofenceId
                });

                
                var selectQuery = @"
                SELECT GeofenceId, FarmId, Latitude, Longitude, Radius, CreatedAt, UpdatedAt
                FROM FarmGeofencing
                WHERE GeofenceId = @GeofenceId";

                return await connection.QuerySingleAsync<FarmGeofencing>(selectQuery, new
                {
                    GeofenceId = request.GeofenceId
                });
            }
        }

        public async Task<bool> DeleteGeofenceAsync(int geofenceId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var checkQuery = @"
            SELECT COUNT(*)
            FROM FarmGeofencing
            WHERE GeofenceId = @GeofenceId";

                var exists = await connection.ExecuteScalarAsync<bool>(checkQuery, new
                {
                    GeofenceId = geofenceId
                });

                if (!exists)
                {
                    return false; 
                }

                var deleteQuery = @"
            DELETE FROM FarmGeofencing
            WHERE GeofenceId = @GeofenceId";

                var rowsAffected = await connection.ExecuteAsync(deleteQuery, new
                {
                    GeofenceId = geofenceId
                });

                return rowsAffected > 0;
            }
        }








    }
}
