// DataAccess/MessageQueue/Repositories/MessageQueueRepository.cs
using Application.MessageQueue.Abstractions;
using Domain.MessageQueues;
using Domain.MessageQueues.Requests;
using Dapper;
using System.Data;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Text;
using System.Text.Json;

namespace DataAccess.MessageQueue.Repositories
{
    public class MessageQueueRepository : IMessageQueueRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public MessageQueueRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<QueuedMessage> EnqueueMessageAsync(QueuedMessageCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO [MessageQueue] (
                        RequestId,
                        RecipientId,
                        ChannelType,
                        MessageContentJson,
                        Priority,
                        Status,
                        ScheduledAt,
                        CreatedAt
                    )
                    OUTPUT INSERTED.*
                    VALUES (
                        @RequestId,
                        @RecipientId,
                        @ChannelType,
                        @MessageContentJson,
                        @Priority,
                        @Status,
                        @ScheduledAt,
                        @CreatedAt
                    );";

                var parameters = new
                {
                    request.RequestId,
                    request.RecipientId,
                    request.ChannelType,
                    MessageContentJson = JsonSerializer.Serialize(request.MessageContentJson),
                    request.Priority,
                    Status = "Queued",
                    request.ScheduledAt,
                    CreatedAt = DateTime.UtcNow
                };

                return await connection.QuerySingleAsync<QueuedMessage>(insertQuery, parameters);
            }
        }

        public async Task<QueuedMessage?> DequeueMessageAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Use transaction to ensure atomic operation
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Get the next message to process
                        var query = @"
                            WITH NextMessage AS (
                                SELECT TOP 1 *
                                FROM [MessageQueue]
                                WHERE Status = 'Queued'
                                AND ScheduledAt <= GETUTCDATE()
                                ORDER BY 
                                    Priority DESC,
                                    CreatedAt ASC
                            )
                            UPDATE NextMessage
                            SET 
                                Status = 'Processing',
                                ProcessedAt = GETUTCDATE()
                            OUTPUT INSERTED.*;";

                        var message = await connection.QuerySingleOrDefaultAsync<QueuedMessage>(query, transaction: transaction);

                        transaction.Commit();
                        return message;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> RequeueMessageAsync(long queueId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageQueue]
                    SET 
                        Status = 'Queued',
                        ProcessedAt = NULL,
                        Priority = Priority + 1
                    WHERE QueueId = @QueueId";

                var affectedRows = await connection.ExecuteAsync(query, new { QueueId = queueId });
                return affectedRows > 0;
            }
        }

        public async Task<QueuedMessage?> GetMessageByIdAsync(long queueId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [MessageQueue]
                    WHERE QueueId = @QueueId";

                return await connection.QuerySingleOrDefaultAsync<QueuedMessage>(query, new { QueueId = queueId });
            }
        }

        public async Task<PagedResultResponse<QueuedMessage>> GetQueuedMessagesAsync(
            int pageNumber,
            int pageSize,
            Guid? requestId = null,
            int? recipientId = null,
            string? channelType = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool? highPriorityFirst = true)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = new StringBuilder(@"
                    SELECT *
                    FROM [MessageQueue]
                    WHERE 1=1");

                if (requestId.HasValue)
                    query.Append(" AND RequestId = @RequestId");

                if (recipientId.HasValue)
                    query.Append(" AND RecipientId = @RecipientId");

                if (!string.IsNullOrWhiteSpace(channelType))
                    query.Append(" AND ChannelType = @ChannelType");

                if (!string.IsNullOrWhiteSpace(status))
                    query.Append(" AND Status = @Status");

                if (fromDate.HasValue)
                    query.Append(" AND CreatedAt >= @FromDate");

                if (toDate.HasValue)
                    query.Append(" AND CreatedAt <= @ToDate");

                query.Append("\nORDER BY ");
                if (highPriorityFirst == true)
                    query.Append("Priority DESC, ");
                query.Append("CreatedAt ASC");

                query.Append(@"
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [MessageQueue]
                    WHERE 1=1");

                // Repeat filters for count query
                if (requestId.HasValue)
                    query.Append(" AND RequestId = @RequestId");

                if (recipientId.HasValue)
                    query.Append(" AND RecipientId = @RecipientId");

                if (!string.IsNullOrWhiteSpace(channelType))
                    query.Append(" AND ChannelType = @ChannelType");

                if (!string.IsNullOrWhiteSpace(status))
                    query.Append(" AND Status = @Status");

                if (fromDate.HasValue)
                    query.Append(" AND CreatedAt >= @FromDate");

                if (toDate.HasValue)
                    query.Append(" AND CreatedAt <= @ToDate");

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    RequestId = requestId,
                    RecipientId = recipientId,
                    ChannelType = channelType,
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate
                }))
                {
                    var messages = multi.Read<QueuedMessage>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<QueuedMessage>
                    {
                        Items = messages,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<bool> MarkMessageAsProcessingAsync(long queueId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageQueue]
                    SET 
                        Status = 'Processing',
                        ProcessedAt = GETUTCDATE()
                    WHERE QueueId = @QueueId";

                var affectedRows = await connection.ExecuteAsync(query, new { QueueId = queueId });
                return affectedRows > 0;
            }
        }

        public async Task<bool> MarkMessageAsCompletedAsync(long queueId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageQueue]
                    SET 
                        Status = 'Completed',
                        ProcessedAt = GETUTCDATE()
                    WHERE QueueId = @QueueId";

                var affectedRows = await connection.ExecuteAsync(query, new { QueueId = queueId });
                return affectedRows > 0;
            }
        }

        public async Task<bool> MarkMessageAsFailedAsync(long queueId, string errorDetails)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageQueue]
                    SET 
                        Status = 'Failed',
                        ProcessedAt = GETUTCDATE(),
                        MessageContentJson = JSON_MODIFY(MessageContentJson, '$.errorDetails', @ErrorDetails)
                    WHERE QueueId = @QueueId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    QueueId = queueId,
                    ErrorDetails = errorDetails
                });
                return affectedRows > 0;
            }
        }

        public async Task<bool> UpdateMessageStatusAsync(long queueId, string status)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageQueue]
                    SET Status = @Status
                    WHERE QueueId = @QueueId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    QueueId = queueId,
                    Status = status
                });
                return affectedRows > 0;
            }
        }

        public async Task<bool> UpdateMessagePriorityAsync(long queueId, int priority)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageQueue]
                    SET Priority = @Priority
                    WHERE QueueId = @QueueId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    QueueId = queueId,
                    Priority = priority
                });
                return affectedRows > 0;
            }
        }

        public async Task<bool> PromoteMessagePriorityAsync(long queueId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageQueue]
                    SET Priority = Priority + 1
                    WHERE QueueId = @QueueId";

                var affectedRows = await connection.ExecuteAsync(query, new { QueueId = queueId });
                return affectedRows > 0;
            }
        }

        public async Task<int> BulkEnqueueMessagesAsync(IEnumerable<QueuedMessageUpdateRequest> messages)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO [MessageQueue] (
                        RequestId,
                        RecipientId,
                        ChannelType,
                        MessageContentJson,
                        Priority,
                        Status,
                        ScheduledAt,
                        CreatedAt
                    )
                    VALUES (
                        @RequestId,
                        @RecipientId,
                        @ChannelType,
                        @MessageContentJson,
                        @Priority,
                        @Status,
                        @ScheduledAt,
                        @CreatedAt
                    );";

                var parameters = messages.Select(m => new
                {
                    m.RequestId,
                    m.RecipientId,
                    m.ChannelType,
                    MessageContentJson = JsonSerializer.Serialize(m.MessageContentJson),
                    m.Priority,
                    Status = "Queued",
                    m.ScheduledAt,
                    CreatedAt = DateTime.UtcNow
                });

                return await connection.ExecuteAsync(insertQuery, parameters);
            }
        }

        public async Task<int> BulkUpdateStatusAsync(IEnumerable<long> queueIds, string status)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageQueue]
                    SET Status = @Status
                    WHERE QueueId IN @QueueIds";

                return await connection.ExecuteAsync(query, new
                {
                    QueueIds = queueIds,
                    Status = status
                });
            }
        }

        public async Task<int> RescheduleStaleMessagesAsync(TimeSpan olderThan, string fromStatus)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var cutoffDate = DateTime.UtcNow.Subtract(olderThan);

                var query = @"
                    UPDATE [MessageQueue]
                    SET 
                        Status = 'Queued',
                        ProcessedAt = NULL,
                        Priority = Priority + 1,
                        ScheduledAt = GETUTCDATE()
                    WHERE Status = @FromStatus
                    AND ProcessedAt < @CutoffDate";

                return await connection.ExecuteAsync(query, new
                {
                    FromStatus = fromStatus,
                    CutoffDate = cutoffDate
                });
            }
        }

        public async Task<int> PurgeProcessedMessagesAsync(DateTime olderThan)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    DELETE FROM [MessageQueue]
                    WHERE Status IN ('Completed', 'Failed')
                    AND ProcessedAt < @OlderThan";

                return await connection.ExecuteAsync(query, new { OlderThan = olderThan });
            }
        }

        public async Task<int> CountQueuedMessagesAsync(string? status = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder("SELECT COUNT(*) FROM [MessageQueue]");
                if (!string.IsNullOrWhiteSpace(status))
                    query.Append(" WHERE Status = @Status");

                return await connection.ExecuteScalarAsync<int>(
                    query.ToString(),
                    new { Status = status });
            }
        }

        public async Task<QueueStatus> GetQueueStatusAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var status = new QueueStatus();

                // Basic counts
                var countQuery = @"
                    SELECT 
                        SUM(CASE WHEN Status = 'Queued' THEN 1 ELSE 0 END) as QueuedCount,
                        SUM(CASE WHEN Status = 'Processing' THEN 1 ELSE 0 END) as ProcessingCount,
                        SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) as FailedCount
                    FROM [MessageQueue]";

                var counts = await connection.QuerySingleAsync(countQuery);
                status.QueuedCount = counts.QueuedCount;
                status.ProcessingCount = counts.ProcessingCount;
                status.FailedCount = counts.FailedCount;

                // Count by channel type
                var channelQuery = @"
                    SELECT ChannelType, COUNT(*) as Count
                    FROM [MessageQueue]
                    GROUP BY ChannelType";

                status.CountByChannelType = (await connection.QueryAsync(channelQuery))
                    .ToDictionary(row => (string)row.ChannelType, row => (int)row.Count);

                // Count by priority
                var priorityQuery = @"
                    SELECT Priority, COUNT(*) as Count
                    FROM [MessageQueue]
                    WHERE Status = 'Queued'
                    GROUP BY Priority";

                status.CountByPriority = (await connection.QueryAsync(priorityQuery))
                    .ToDictionary(row => (int)row.Priority, row => (int)row.Count);

                return status;
            }
        }
    }
}