// DataAccess/Notifications/Repositories/NotificationsRepository.cs
using Application.Notifications.Abstractions;
using Domain.Notifications.Models;
using Domain.Notifications.Requests;
using Dapper;
using System.Data;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Text;

namespace DataAccess.Notifications.Repositories
{
    public class NotificationsRepository : INotificationsRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public NotificationsRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<Notification> CreateNotificationAsync(NotificationCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Insert the new notification
                var insertQuery = @"
                    INSERT INTO [Notifications] (UserId, Title, Body, Status, CreatedAt, SentAt)
                    VALUES (@UserId, @Title, @Body, @Status, @CreatedAt, @SentAt);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Body = request.Body,
                    Status = "Pending", // Default status
                    CreatedAt = DateTime.UtcNow,
                    SentAt = (DateTime?)null
                };

                var notificationId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                // Return the created notification object
                return new Notification
                {
                    NotificationId = notificationId,
                    UserId = request.UserId,
                    Title = request.Title,
                    Body = request.Body,
                    Status = "Pending",
                    CreatedAt = parameters.CreatedAt,
                    SentAt = parameters.SentAt
                };
            }
        }

        public async Task<PagedResultResponse<Notification>> GetNotificationsAsync(
    int pageNumber,
    int pageSize,
    string? search = null,
    int? userId = null,
    string? status = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
            SELECT *
            FROM [Notifications]
            WHERE 1=1");

                // Add optional filters
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Title LIKE @Search
                OR Body LIKE @Search)");
                }

                if (userId.HasValue)
                {
                    query.Append(@"
                AND UserId = @UserId");
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query.Append(@"
                AND Status = @Status");
                }

                // Order by: Pending notifications first, then by CreatedAt (latest first)
                query.Append(@"
            ORDER BY 
                CASE 
                    WHEN Status = 'Pending' THEN 1 
                    ELSE 2 
                END, 
                CreatedAt DESC
            OFFSET @Skip ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM [Notifications]
            WHERE 1=1");

                // Repeat optional filters for the count query
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Title LIKE @Search
                OR Body LIKE @Search)");
                }

                if (userId.HasValue)
                {
                    query.Append(@"
                AND UserId = @UserId");
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query.Append(@"
                AND Status = @Status");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    UserId = userId,
                    Status = status
                }))
                {
                    var notifications = multi.Read<Notification>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Notification>
                    {
                        Items = notifications,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [Notifications]
                    WHERE NotificationId = @NotificationId";

                return await connection.QuerySingleOrDefaultAsync<Notification>(query, new { NotificationId = notificationId });
            }
        }

        public async Task<Notification> UpdateNotificationAsync(NotificationUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the notification exists
                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [Notifications]
                    WHERE NotificationId = @NotificationId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { NotificationId = request.NotificationId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException(request.NotificationId);
                }

                // Prepare the SQL query to update the notification
                var updateQuery = @"
                    UPDATE [Notifications]
                    SET Status = @Status,
                        SentAt = @SentAt
                    WHERE NotificationId = @NotificationId";

                // Prepare the parameters
                var parameters = new
                {
                    NotificationId = request.NotificationId,
                    Status = request.Status,
                    SentAt = request.SentAt
                };

                // Execute the update
                await connection.ExecuteAsync(updateQuery, parameters);

                // Retrieve the updated notification details
                var query = @"
                    SELECT *
                    FROM [Notifications]
                    WHERE NotificationId = @NotificationId";

                var notification = await connection.QuerySingleOrDefaultAsync<Notification>(query, new { NotificationId = request.NotificationId });

                if (notification == null)
                {
                    throw new ItemDoesNotExistException(request.NotificationId);
                }

                return notification;
            }
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the notification exists
                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [Notifications]
                    WHERE NotificationId = @NotificationId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { NotificationId = notificationId });

                if (exists == 0)
                {
                    return false;
                }

                // Delete the notification
                var deleteQuery = @"
                    DELETE FROM [Notifications]
                    WHERE NotificationId = @NotificationId";

                await connection.ExecuteAsync(deleteQuery, new { NotificationId = notificationId });

                return true;
            }
        }

        public async Task<int> CountNotificationsAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [Notifications]";

                return await connection.ExecuteScalarAsync<int>(query);
            }
        }

        
    }
}