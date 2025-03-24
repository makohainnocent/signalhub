// DataAccess/UserDevices/Repositories/UserDevicesRepository.cs
using Application.UserDevices.Abstractions;
using Domain.UserDevices.Models;
using Domain.UserDevices.Requests;
using Dapper;
using System.Data;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Text;

namespace DataAccess.UserDevices.Repositories
{
    public class UserDevicesRepository : IUserDevicesRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public UserDevicesRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<UserDevice> CreateUserDeviceAsync(UserDeviceCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the device token already exists for the user
                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [UserDevices]
                    WHERE UserId = @UserId AND DeviceToken = @DeviceToken";

                var existingCount = await connection.QuerySingleAsync<int>(checkQuery, new
                {
                    UserId = request.UserId,
                    DeviceToken = request.DeviceToken
                });

                if (existingCount > 0)
                {
                    throw new ItemAlreadyExistsException("Device token already registered for this user.");
                }

                // Insert the new user device
                var insertQuery = @"
                    INSERT INTO [UserDevices] (UserId, DeviceToken, Platform, CreatedAt, UpdatedAt)
                    VALUES (@UserId, @DeviceToken, @Platform, @CreatedAt, @UpdatedAt);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    UserId = request.UserId,
                    DeviceToken = request.DeviceToken,
                    Platform = request.Platform,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null
                };

                var deviceId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                // Return the created user device object
                return new UserDevice
                {
                    DeviceId = deviceId,
                    UserId = request.UserId,
                    DeviceToken = request.DeviceToken,
                    Platform = request.Platform,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt
                };
            }
        }

        public async Task<PagedResultResponse<UserDevice>> GetUserDevicesAsync(int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                    SELECT *
                    FROM [UserDevices]
                    WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                        AND (DeviceToken LIKE @Search
                        OR Platform LIKE @Search)");
                }

                query.Append(@"
                    ORDER BY CreatedAt DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [UserDevices]
                    WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                        AND (DeviceToken LIKE @Search
                        OR Platform LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var userDevices = multi.Read<UserDevice>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<UserDevice>
                    {
                        Items = userDevices,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<UserDevice?> GetUserDeviceByIdAsync(int deviceId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [UserDevices]
                    WHERE DeviceId = @DeviceId";

                return await connection.QuerySingleOrDefaultAsync<UserDevice>(query, new { DeviceId = deviceId });
            }
        }

        public async Task<UserDevice> UpdateUserDeviceAsync(UserDeviceUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the user device exists
                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [UserDevices]
                    WHERE DeviceId = @DeviceId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { DeviceId = request.DeviceId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException(request.DeviceId);
                }

                // Prepare the SQL query to update the user device
                var updateQuery = @"
                    UPDATE [UserDevices]
                    SET DeviceToken = @DeviceToken,
                        Platform = @Platform,
                        UpdatedAt = @UpdatedAt
                    WHERE DeviceId = @DeviceId";

                // Prepare the parameters
                var parameters = new
                {
                    DeviceId = request.DeviceId,
                    DeviceToken = request.DeviceToken,
                    Platform = request.Platform,
                    UpdatedAt = DateTime.UtcNow
                };

                // Execute the update
                await connection.ExecuteAsync(updateQuery, parameters);

                // Retrieve the updated user device details
                var query = @"
                    SELECT *
                    FROM [UserDevices]
                    WHERE DeviceId = @DeviceId";

                var userDevice = await connection.QuerySingleOrDefaultAsync<UserDevice>(query, new { DeviceId = request.DeviceId });

                if (userDevice == null)
                {
                    throw new ItemDoesNotExistException(request.DeviceId);
                }

                return userDevice;
            }
        }

        public async Task<bool> DeleteUserDeviceAsync(int deviceId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the user device exists
                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [UserDevices]
                    WHERE DeviceId = @DeviceId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { DeviceId = deviceId });

                if (exists == 0)
                {
                    return false;
                }

                // Delete the user device
                var deleteQuery = @"
                    DELETE FROM [UserDevices]
                    WHERE DeviceId = @DeviceId";

                await connection.ExecuteAsync(deleteQuery, new { DeviceId = deviceId });

                return true;
            }
        }

        public async Task<int> CountUserDevicesAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [UserDevices]";

                return await connection.ExecuteScalarAsync<int>(query);
            }
        }
    }
}