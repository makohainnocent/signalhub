// DataAccess/DeliveryLogs/Repositories/DeliveryLogsRepository.cs
using Application.DeliveryLogs.Abstractions;
using Domain.DeliveryLogs;
using Domain.DeliveryLogs.Requests;
using Dapper;
using System.Data;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace DataAccess.DeliveryLogs.Repositories
{
    public class DeliveryLogsRepository : IDeliveryLogsRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public DeliveryLogsRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<DeliveryLog> CreateLogAsync(DeliveryLogCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO [DeliveryLogs] (DeliveryId, EventType, EventDataJson, CreatedAt)
                    VALUES (@DeliveryId, @EventType, @EventDataJson, @CreatedAt);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    request.DeliveryId,
                    request.EventType,
                    EventDataJson = JsonSerializer.Serialize(request.EventDataJson),
                    CreatedAt = DateTime.UtcNow
                };

                var logId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new DeliveryLog
                {
                    LogId = logId,
                    DeliveryId = request.DeliveryId,
                    EventType = request.EventType,
                    EventDataJson = parameters.EventDataJson,
                    CreatedAt = parameters.CreatedAt
                };
            }
        }

        public async Task<int> BulkCreateLogsAsync(IEnumerable<DeliveryLogCreationRequest> logs)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO [DeliveryLogs] (DeliveryId, EventType, EventDataJson, CreatedAt)
                    VALUES (@DeliveryId, @EventType, @EventDataJson, @CreatedAt);";

                var parameters = logs.Select(log => new
                {
                    log.DeliveryId,
                    log.EventType,
                    EventDataJson = JsonSerializer.Serialize(log.EventDataJson),
                    CreatedAt = DateTime.UtcNow
                });

                return await connection.ExecuteAsync(insertQuery, parameters);
            }
        }

        public async Task<DeliveryLog?> GetLogByIdAsync(int logId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [DeliveryLogs]
                    WHERE LogId = @LogId";

                return await connection.QuerySingleOrDefaultAsync<DeliveryLog>(query, new { LogId = logId });
            }
        }

        public async Task<PagedResultResponse<DeliveryLog>> GetLogsByDeliveryAsync(
            int deliveryId,
            int pageNumber,
            int pageSize,
            string? eventType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                    SELECT *
                    FROM [DeliveryLogs]
                    WHERE DeliveryId = @DeliveryId");

                if (!string.IsNullOrWhiteSpace(eventType))
                {
                    query.Append(" AND EventType = @EventType");
                }

                if (fromDate.HasValue)
                {
                    query.Append(" AND CreatedAt >= @FromDate");
                }

                if (toDate.HasValue)
                {
                    query.Append(" AND CreatedAt <= @ToDate");
                }

                query.Append(@"
                    ORDER BY CreatedAt DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [DeliveryLogs]
                    WHERE DeliveryId = @DeliveryId");

                if (!string.IsNullOrWhiteSpace(eventType))
                {
                    query.Append(" AND EventType = @EventType");
                }

                if (fromDate.HasValue)
                {
                    query.Append(" AND CreatedAt >= @FromDate");
                }

                if (toDate.HasValue)
                {
                    query.Append(" AND CreatedAt <= @ToDate");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    DeliveryId = deliveryId,
                    Skip = skip,
                    PageSize = pageSize,
                    EventType = eventType,
                    FromDate = fromDate,
                    ToDate = toDate
                }))
                {
                    var logs = multi.Read<DeliveryLog>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<DeliveryLog>
                    {
                        Items = logs,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<List<DeliveryLog>> GetErrorLogsAsync(int deliveryId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [DeliveryLogs]
                    WHERE DeliveryId = @DeliveryId 
                    AND EventType IN ('DeliveryFailed', 'ProviderError', 'SystemError')
                    ORDER BY CreatedAt DESC";

                return (await connection.QueryAsync<DeliveryLog>(query, new { DeliveryId = deliveryId })).ToList();
            }
        }

        public async Task<List<DeliveryLog>> GetStatusTransitionLogsAsync(int deliveryId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [DeliveryLogs]
                    WHERE DeliveryId = @DeliveryId 
                    AND EventType = 'StatusChanged'
                    ORDER BY CreatedAt DESC";

                return (await connection.QueryAsync<DeliveryLog>(query, new { DeliveryId = deliveryId })).ToList();
            }
        }

        public async Task<DeliveryLog?> GetLastProviderResponseAsync(int deliveryId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT TOP 1 *
                    FROM [DeliveryLogs]
                    WHERE DeliveryId = @DeliveryId 
                    AND EventType = 'ProviderResponse'
                    ORDER BY CreatedAt DESC";

                return await connection.QuerySingleOrDefaultAsync<DeliveryLog>(query, new { DeliveryId = deliveryId });
            }
        }

        /*public async Task<DeliveryTimeline> GetDeliveryTimelineAsync(int deliveryId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var timeline = new DeliveryTimeline();

                // Get key events
                var keyEventsQuery = @"
            SELECT EventType, MIN(CreatedAt) as FirstOccurrence, MAX(CreatedAt) as LastOccurrence
            FROM [DeliveryLogs]
            WHERE DeliveryId = @DeliveryId
            AND EventType IN ('Queued', 'DeliveryAttempt', 'Delivered')
            GROUP BY EventType";

                var keyEvents = await connection.QueryAsync(keyEventsQuery, new { DeliveryId = deliveryId });

                foreach (var evt in keyEvents)
                {
                    switch (evt.EventType)
                    {
                        case "Queued":
                            timeline.QueuedTime = evt.FirstOccurrence;
                            break;
                        case "Delivered":
                            timeline.DeliveredTime = evt.FirstOccurrence;
                            break;
                    }
                }

                // Get all attempts
                var attemptsQuery = @"
            SELECT EventDataJson, CreatedAt as AttemptTime
            FROM [DeliveryLogs]
            WHERE DeliveryId = @DeliveryId
            AND EventType = 'DeliveryAttempt'
            ORDER BY CreatedAt";

                var attempts = await connection.QueryAsync(attemptsQuery, new { DeliveryId = deliveryId });

                timeline.Attempts = attempts.Select(a =>
                {
                    var data = JsonSerializer.Deserialize<Dictionary<string, object>>(a.EventDataJson);
                    var attempt = new DeliveryAttempt
                    {
                        AttemptTime = a.AttemptTime,
                        ProviderResponse = string.Empty,
                        Success = false,
                        Duration = TimeSpan.Zero
                    };

                    if (data.TryGetValue("ProviderResponse", out var response) && response != null)
                    {
                        attempt.ProviderResponse = response.ToString();
                    }

                    if (data.TryGetValue("Success", out var success) && success != null)
                    {
                        attempt.Success = Convert.ToBoolean(success);
                    }

                    if (data.TryGetValue("Duration", out var duration) && duration != null)
                    {
                        attempt.Duration = TimeSpan.Parse(duration.ToString());
                    }

                    return attempt;
                }).ToList();

                if (timeline.Attempts.Any())
                {
                    timeline.FirstAttempt = timeline.Attempts.First().AttemptTime;
                    timeline.LastAttempt = timeline.Attempts.Last().AttemptTime;
                }

                // Get status changes
                var statusChangesQuery = @"
            SELECT EventDataJson, CreatedAt as ChangeTime
            FROM [DeliveryLogs]
            WHERE DeliveryId = @DeliveryId
            AND EventType = 'StatusChanged'
            ORDER BY CreatedAt";

                var statusChanges = await connection.QueryAsync(statusChangesQuery, new { DeliveryId = deliveryId });

                timeline.StatusChanges = statusChanges.Select(sc =>
                {
                    var data = JsonSerializer.Deserialize<Dictionary<string, string>>(sc.EventDataJson);
                    return new StatusChange
                    {
                        ChangeTime = sc.ChangeTime,
                        FromStatus = data["FromStatus"],
                        ToStatus = data["ToStatus"]
                    };
                }).ToList();

                return timeline;
            }
        }*/

        public async Task<Dictionary<string, int>> GetEventTypeDistributionAsync(
            int? deliveryId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder(@"
                    SELECT EventType, COUNT(*) as Count
                    FROM [DeliveryLogs]
                    WHERE 1=1");

                if (deliveryId.HasValue)
                {
                    query.Append(" AND DeliveryId = @DeliveryId");
                }

                query.Append(" GROUP BY EventType");

                var results = await connection.QueryAsync(query.ToString(), new
                {
                    DeliveryId = deliveryId
                });

                return results.ToDictionary(r => (string)r.EventType, r => (int)r.Count);
            }
        }

        public async Task<List<CommonError>> GetFrequentErrorsAsync(
            TimeSpan period,
            int? providerId = null,
            string? channelType = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT 
                        JSON_VALUE(EventDataJson, '$.ErrorType') as ErrorType,
                        JSON_VALUE(EventDataJson, '$.ErrorMessage') as ErrorMessage,
                        COUNT(*) as OccurrenceCount,
                        MAX(CreatedAt) as LastOccurred
                    FROM [DeliveryLogs]
                    WHERE EventType IN ('DeliveryFailed', 'ProviderError', 'SystemError')
                    AND CreatedAt >= @SinceDate";

                if (providerId.HasValue)
                {
                    query += " AND JSON_VALUE(EventDataJson, '$.ProviderId') = @ProviderId";
                }

                if (!string.IsNullOrWhiteSpace(channelType))
                {
                    query += " AND JSON_VALUE(EventDataJson, '$.ChannelType') = @ChannelType";
                }

                query += @"
                    GROUP BY 
                        JSON_VALUE(EventDataJson, '$.ErrorType'),
                        JSON_VALUE(EventDataJson, '$.ErrorMessage')
                    ORDER BY COUNT(*) DESC";

                var sinceDate = DateTime.UtcNow.Subtract(period);

                return (await connection.QueryAsync<CommonError>(query, new
                {
                    SinceDate = sinceDate,
                    ProviderId = providerId?.ToString(),
                    ChannelType = channelType
                })).ToList();
            }
        }

        public async Task<int> ArchiveLogsAsync(DateTime cutoffDate)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // First copy to archive table
                var archiveQuery = @"
                    INSERT INTO [DeliveryLogsArchive]
                    SELECT * FROM [DeliveryLogs]
                    WHERE CreatedAt < @CutoffDate";

                var archivedCount = await connection.ExecuteAsync(archiveQuery, new { CutoffDate = cutoffDate });

                // Then delete from main table
                var deleteQuery = @"
                    DELETE FROM [DeliveryLogs]
                    WHERE CreatedAt < @CutoffDate";

                await connection.ExecuteAsync(deleteQuery, new { CutoffDate = cutoffDate });

                return archivedCount;
            }
        }

        public async Task<int> CompressLogDataAsync(TimeSpan olderThan)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var cutoffDate = DateTime.UtcNow.Subtract(olderThan);

                var query = @"
                    UPDATE [DeliveryLogs]
                    SET EventDataJson = JSON_MODIFY(
                        JSON_MODIFY(EventDataJson, '$.compressed', 1),
                        '$.originalLength', DATALENGTH(EventDataJson)
                    )
                    WHERE CreatedAt < @CutoffDate
                    AND JSON_VALUE(EventDataJson, '$.compressed') IS NULL";

                return await connection.ExecuteAsync(query, new { CutoffDate = cutoffDate });
            }
        }

        public Task<PagedResultResponse<DeliveryLog>> GetLogsByRequestAsync(Guid requestId, int pageNumber, int pageSize, string? eventType = null)
        {
            throw new NotImplementedException("RequestId-based queries are not supported in the current model");
        }
    }
}