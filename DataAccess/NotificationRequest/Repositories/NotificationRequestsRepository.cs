// DataAccess/NotificationRequests/Repositories/NotificationRequestsRepository.cs
using Application.NotificationRequests.Abstractions;
using Domain.NotificationRequests;
using Domain.NotificationRequests.Requests;
using Dapper;
using System.Data;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Text;
using System.Text.Json;

namespace DataAccess.NotificationRequests.Repositories
{
    public class NotificationRequestsRepository : INotificationRequestsRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public NotificationRequestsRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<NotificationRequest> CreateRequestAsync(NotificationRequestCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO [NotificationRequests] (
                        RequestId,
                        ApplicationId,
                        TemplateId,
                        RequestDataJson,
                        Priority,
                        Status,
                        CreatedAt,
                        ExpirationAt,
                        CallbackUrl,
                        RequestedByUserId
                    )
                    OUTPUT INSERTED.*
                    VALUES (
                        @RequestId,
                        @ApplicationId,
                        @TemplateId,
                        @RequestDataJson,
                        @Priority,
                        @Status,
                        @CreatedAt,
                        @ExpirationAt,
                        @CallbackUrl,
                        @RequestedByUserId
                    );";

                var parameters = new
                {
                    RequestId = Guid.NewGuid(),
                    request.ApplicationId,
                    request.TemplateId,
                    RequestDataJson = JsonSerializer.Serialize(request.RequestDataJson),
                    request.Priority,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    request.ExpirationAt,
                    request.CallbackUrl,
                    request.RequestedByUserId
                };

                return await connection.QuerySingleAsync<NotificationRequest>(insertQuery, parameters);
            }
        }

        public async Task<PagedResultResponse<NotificationRequest>> GetRequestsAsync(
            int pageNumber,
            int pageSize,
            int? applicationId = null,
            int? templateId = null,
            string? status = null,
            string? priority = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? requestedByUserId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = new StringBuilder(@"
                    SELECT *
                    FROM [NotificationRequests]
                    WHERE 1=1");

                if (applicationId.HasValue)
                    query.Append(" AND ApplicationId = @ApplicationId");

                if (templateId.HasValue)
                    query.Append(" AND TemplateId = @TemplateId");

                if (!string.IsNullOrWhiteSpace(status))
                    query.Append(" AND Status = @Status");

                if (!string.IsNullOrWhiteSpace(priority))
                    query.Append(" AND Priority = @Priority");

                if (fromDate.HasValue)
                    query.Append(" AND CreatedAt >= @FromDate");

                if (toDate.HasValue)
                    query.Append(" AND CreatedAt <= @ToDate");

                if (requestedByUserId.HasValue)
                    query.Append(" AND RequestedByUserId = @RequestedByUserId");

                query.Append(@"
                    ORDER BY 
                        CASE Priority
                            WHEN 'High' THEN 1
                            WHEN 'Normal' THEN 2
                            WHEN 'Low' THEN 3
                            ELSE 4
                        END,
                        CreatedAt DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [NotificationRequests]
                    WHERE 1=1");

                // Repeat filters for count query
                if (applicationId.HasValue)
                    query.Append(" AND ApplicationId = @ApplicationId");

                if (templateId.HasValue)
                    query.Append(" AND TemplateId = @TemplateId");

                if (!string.IsNullOrWhiteSpace(status))
                    query.Append(" AND Status = @Status");

                if (!string.IsNullOrWhiteSpace(priority))
                    query.Append(" AND Priority = @Priority");

                if (fromDate.HasValue)
                    query.Append(" AND CreatedAt >= @FromDate");

                if (toDate.HasValue)
                    query.Append(" AND CreatedAt <= @ToDate");

                if (requestedByUserId.HasValue)
                    query.Append(" AND RequestedByUserId = @RequestedByUserId");

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    ApplicationId = applicationId,
                    TemplateId = templateId,
                    Status = status,
                    Priority = priority,
                    FromDate = fromDate,
                    ToDate = toDate,
                    RequestedByUserId = requestedByUserId
                }))
                {
                    var requests = multi.Read<NotificationRequest>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<NotificationRequest>
                    {
                        Items = requests,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<NotificationRequest?> GetRequestByIdAsync(Guid requestId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [NotificationRequests]
                    WHERE RequestId = @RequestId";

                return await connection.QuerySingleOrDefaultAsync<NotificationRequest>(query, new { RequestId = requestId });
            }
        }

        public async Task<int> CountRequestsAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [NotificationRequests]";
                return await connection.ExecuteScalarAsync<int>(query);
            }
        }

        public async Task<bool> MarkRequestAsProcessingAsync(Guid requestId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET 
                        Status = 'Processing',
                        ProcessedAt = GETUTCDATE()
                    WHERE RequestId = @RequestId";

                var affectedRows = await connection.ExecuteAsync(query, new { RequestId = requestId });
                return affectedRows > 0;
            }
        }

        public async Task<bool> MarkRequestAsCompletedAsync(Guid requestId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET 
                        Status = 'Completed',
                        ProcessedAt = GETUTCDATE()
                    WHERE RequestId = @RequestId";

                var affectedRows = await connection.ExecuteAsync(query, new { RequestId = requestId });
                return affectedRows > 0;
            }
        }

        public async Task<bool> MarkRequestAsFailedAsync(Guid requestId, string errorDetails)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET 
                        Status = 'Failed',
                        ProcessedAt = GETUTCDATE(),
                        RequestDataJson = JSON_MODIFY(RequestDataJson, '$.errorDetails', @ErrorDetails)
                    WHERE RequestId = @RequestId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    RequestId = requestId,
                    ErrorDetails = errorDetails
                });
                return affectedRows > 0;
            }
        }

        public async Task<bool> CancelRequestAsync(Guid requestId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET 
                        Status = 'Cancelled',
                        ProcessedAt = GETUTCDATE()
                    WHERE RequestId = @RequestId
                    AND Status IN ('Pending', 'Processing')";

                var affectedRows = await connection.ExecuteAsync(query, new { RequestId = requestId });
                return affectedRows > 0;
            }
        }

        public async Task<bool> UpdateRequestStatusAsync(Guid requestId, string status)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET Status = @Status
                    WHERE RequestId = @RequestId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    RequestId = requestId,
                    Status = status
                });
                return affectedRows > 0;
            }
        }

        public async Task<bool> UpdateRequestPriorityAsync(Guid requestId, string priority)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET Priority = @Priority
                    WHERE RequestId = @RequestId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    RequestId = requestId,
                    Priority = priority
                });
                return affectedRows > 0;
            }
        }

        public async Task<bool> SetRequestExpirationAsync(Guid requestId, DateTime expirationAt)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET ExpirationAt = @ExpirationAt
                    WHERE RequestId = @RequestId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    RequestId = requestId,
                    ExpirationAt = expirationAt
                });
                return affectedRows > 0;
            }
        }

        public async Task<List<NotificationRequest>> GetExpiredRequestsAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [NotificationRequests]
                    WHERE ExpirationAt IS NOT NULL
                    AND ExpirationAt <= GETUTCDATE()
                    AND Status IN ('Pending', 'Processing')";

                return (await connection.QueryAsync<NotificationRequest>(query)).ToList();
            }
        }

        public async Task<bool> UpdateCallbackUrlAsync(Guid requestId, string callbackUrl)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET CallbackUrl = @CallbackUrl
                    WHERE RequestId = @RequestId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    RequestId = requestId,
                    CallbackUrl = callbackUrl
                });
                return affectedRows > 0;
            }
        }

        public async Task<bool> TriggerCallbackAsync(Guid requestId, string callbackData)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET 
                        RequestDataJson = JSON_MODIFY(RequestDataJson, '$.callbackData', @CallbackData),
                        RequestDataJson = JSON_MODIFY(RequestDataJson, '$.callbackTriggeredAt', GETUTCDATE())
                    WHERE RequestId = @RequestId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    RequestId = requestId,
                    CallbackData = callbackData
                });
                return affectedRows > 0;
            }
        }

        public async Task<string> GetRequestDataAsync(Guid requestId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT RequestDataJson
                    FROM [NotificationRequests]
                    WHERE RequestId = @RequestId";

                return await connection.QuerySingleOrDefaultAsync<string>(query, new { RequestId = requestId });
            }
        }

        public async Task<bool> UpdateRequestDataAsync(Guid requestId, string requestDataJson)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET RequestDataJson = @RequestDataJson
                    WHERE RequestId = @RequestId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    RequestId = requestId,
                    RequestDataJson = requestDataJson
                });
                return affectedRows > 0;
            }
        }

        public async Task<int> BulkUpdateStatusAsync(IEnumerable<Guid> requestIds, string status)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET Status = @Status
                    WHERE RequestId IN @RequestIds";

                return await connection.ExecuteAsync(query, new
                {
                    RequestIds = requestIds,
                    Status = status
                });
            }
        }

        public async Task<int> BulkCancelRequestsAsync(IEnumerable<Guid> requestIds)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [NotificationRequests]
                    SET 
                        Status = 'Cancelled',
                        ProcessedAt = GETUTCDATE()
                    WHERE RequestId IN @RequestIds
                    AND Status IN ('Pending', 'Processing')";

                return await connection.ExecuteAsync(query, new { RequestIds = requestIds });
            }
        }
    }
}