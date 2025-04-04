// DataAccess/EventLogs/Repositories/EventLogsRepository.cs
using Application.EventLogs.Abstractions;
using Domain.EventLogs;
using Domain.EventLogs.Requests;
using Dapper;
using System.Data;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Text;
using System.Text.Json;

namespace DataAccess.EventLogs.Repositories
{
    public class EventLogsRepository : IEventLogsRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public EventLogsRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        

        public async Task<EventLog> LogEventAsync(EventLogCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO [EventLogs] (
                        EntityType, 
                        EntityId, 
                        EventType, 
                        EventDataJson, 
                        CreatedAt, 
                        CreatedByUserId
                    )
                    VALUES (
                        @EntityType, 
                        @EntityId, 
                        @EventType, 
                        @EventDataJson, 
                        @CreatedAt, 
                        @CreatedByUserId
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    request.EntityType,
                    request.EntityId,
                    request.EventType,
                    EventDataJson = JsonSerializer.Serialize(request.EventDataJson),
                    CreatedAt = DateTime.UtcNow,
                    request.CreatedByUserId
                };

                var eventId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new EventLog
                {
                    EventId = eventId,
                    EntityType = request.EntityType,
                    EntityId = request.EntityId,
                    EventType = request.EventType,
                    EventDataJson = parameters.EventDataJson,
                    CreatedAt = parameters.CreatedAt,
                    CreatedByUserId = request.CreatedByUserId
                };
            }
        }

        public async Task<int> BulkLogEventsAsync(IEnumerable<EventLogCreationRequest> events)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO [EventLogs] (
                        EntityType, 
                        EntityId, 
                        EventType, 
                        EventDataJson, 
                        CreatedAt, 
                        CreatedByUserId
                    )
                    VALUES (
                        @EntityType, 
                        @EntityId, 
                        @EventType, 
                        @EventDataJson, 
                        @CreatedAt, 
                        @CreatedByUserId
                    );";

                var parameters = events.Select(e => new
                {
                    e.EntityType,
                    e.EntityId,
                    e.EventType,
                    EventDataJson = JsonSerializer.Serialize(e.EventDataJson),
                    CreatedAt = DateTime.UtcNow,
                    e.CreatedByUserId
                });

                return await connection.ExecuteAsync(insertQuery, parameters);
            }
        }

        public async Task<EventLog?> GetEventByIdAsync(int eventId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [EventLogs]
                    WHERE EventId = @EventId";

                return await connection.QuerySingleOrDefaultAsync<EventLog>(query, new { EventId = eventId });
            }
        }

        public async Task<PagedResultResponse<EventLog>> GetEventsAsync(
            int pageNumber,
            int pageSize,
            string? entityType = null,
            string? entityId = null,
            string? eventType = null,
            int? createdByUserId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? searchQuery = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = new StringBuilder(@"
                    SELECT *
                    FROM [EventLogs]
                    WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(entityType))
                    query.Append(" AND EntityType = @EntityType");

                if (!string.IsNullOrWhiteSpace(entityId))
                    query.Append(" AND EntityId = @EntityId");

                if (!string.IsNullOrWhiteSpace(eventType))
                    query.Append(" AND EventType = @EventType");

                if (createdByUserId.HasValue)
                    query.Append(" AND CreatedByUserId = @CreatedByUserId");

                if (fromDate.HasValue)
                    query.Append(" AND CreatedAt >= @FromDate");

                if (toDate.HasValue)
                    query.Append(" AND CreatedAt <= @ToDate");

                if (!string.IsNullOrWhiteSpace(searchQuery))
                    query.Append(" AND EventDataJson LIKE @SearchQuery");

                query.Append(@"
                    ORDER BY CreatedAt DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [EventLogs]
                    WHERE 1=1");

                // Repeat filters for count query
                if (!string.IsNullOrWhiteSpace(entityType))
                    query.Append(" AND EntityType = @EntityType");

                if (!string.IsNullOrWhiteSpace(entityId))
                    query.Append(" AND EntityId = @EntityId");

                if (!string.IsNullOrWhiteSpace(eventType))
                    query.Append(" AND EventType = @EventType");

                if (createdByUserId.HasValue)
                    query.Append(" AND CreatedByUserId = @CreatedByUserId");

                if (fromDate.HasValue)
                    query.Append(" AND CreatedAt >= @FromDate");

                if (toDate.HasValue)
                    query.Append(" AND CreatedAt <= @ToDate");

                if (!string.IsNullOrWhiteSpace(searchQuery))
                    query.Append(" AND EventDataJson LIKE @SearchQuery");

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    EntityType = entityType,
                    EntityId = entityId,
                    EventType = eventType,
                    CreatedByUserId = createdByUserId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    SearchQuery = $"%{searchQuery}%"
                }))
                {
                    var events = multi.Read<EventLog>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<EventLog>
                    {
                        Items = events,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<PagedResultResponse<EventLog>> GetEntityActivityAsync(
            string entityType,
            string entityId,
            int pageNumber,
            int pageSize)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = new StringBuilder(@"
                    SELECT *
                    FROM [EventLogs]
                    WHERE EntityType = @EntityType
                    AND EntityId = @EntityId
                    ORDER BY CreatedAt DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [EventLogs]
                    WHERE EntityType = @EntityType
                    AND EntityId = @EntityId");

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var events = multi.Read<EventLog>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<EventLog>
                    {
                        Items = events,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<List<EventLog>> GetEntityTimelineAsync(
            string entityType,
            string entityId,
            int limit = 100)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT TOP (@Limit) *
                    FROM [EventLogs]
                    WHERE EntityType = @EntityType
                    AND EntityId = @EntityId
                    ORDER BY CreatedAt DESC";

                return (await connection.QueryAsync<EventLog>(query, new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Limit = limit
                })).ToList();
            }
        }

        public async Task<PagedResultResponse<EventLog>> GetUserActivityAsync(
            int userId,
            int pageNumber,
            int pageSize,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            return await GetEventsAsync(
                pageNumber,
                pageSize,
                createdByUserId: userId,
                fromDate: fromDate,
                toDate: toDate);
        }

        //public async Task<EventStatistics> GetEventStatisticsAsync(
        //    DateTime? startDate = null,
        //    DateTime? endDate = null)
        //{
        //    using (var connection = _dbConnectionProvider.CreateConnection())
        //    {
        //        connection.Open();

        //        var statistics = new EventStatistics();

        //        // Total events count
        //        var totalQuery = new StringBuilder("SELECT COUNT(*) FROM [EventLogs] WHERE 1=1");
        //        if (startDate.HasValue)
        //            totalQuery.Append(" AND CreatedAt >= @StartDate");
        //        if (endDate.HasValue)
        //            totalQuery.Append(" AND CreatedAt <= @EndDate");

        //        statistics.TotalEvents = await connection.ExecuteScalarAsync<int>(totalQuery.ToString(), new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate
        //        });

        //        // Events by type
        //        var typeQuery = new StringBuilder(@"
        //            SELECT EventType, COUNT(*) as Count
        //            FROM [EventLogs]
        //            WHERE 1=1");
        //        if (startDate.HasValue)
        //            typeQuery.Append(" AND CreatedAt >= @StartDate");
        //        if (endDate.HasValue)
        //            typeQuery.Append(" AND CreatedAt <= @EndDate");
        //        typeQuery.Append(" GROUP BY EventType");

        //        statistics.EventsByType = (await connection.QueryAsync(typeQuery.ToString(), new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate
        //        })).ToDictionary(row => (string)row.EventType, row => (int)row.Count);

        //        // Events by entity
        //        var entityQuery = new StringBuilder(@"
        //            SELECT EntityType, COUNT(*) as Count
        //            FROM [EventLogs]
        //            WHERE 1=1");
        //        if (startDate.HasValue)
        //            entityQuery.Append(" AND CreatedAt >= @StartDate");
        //        if (endDate.HasValue)
        //            entityQuery.Append(" AND CreatedAt <= @EndDate");
        //        entityQuery.Append(" GROUP BY EntityType");

        //        statistics.EventsByEntity = (await connection.QueryAsync(entityQuery.ToString(), new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate
        //        })).ToDictionary(row => (string)row.EntityType, row => (int)row.Count);

        //        // Events by user
        //        var userQuery = new StringBuilder(@"
        //            SELECT CreatedByUserId, COUNT(*) as Count
        //            FROM [EventLogs]
        //            WHERE CreatedByUserId IS NOT NULL");
        //        if (startDate.HasValue)
        //            userQuery.Append(" AND CreatedAt >= @StartDate");
        //        if (endDate.HasValue)
        //            userQuery.Append(" AND CreatedAt <= @EndDate");
        //        userQuery.Append(" GROUP BY CreatedByUserId");

        //        statistics.EventsByUser = (await connection.QueryAsync(userQuery.ToString(), new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate
        //        })).ToDictionary(row => row.CreatedByUserId.ToString(), row => (int)row.Count);

        //        // Hourly distribution
        //        var hourlyQuery = new StringBuilder(@"
        //            SELECT DATEPART(HOUR, CreatedAt) as Hour, COUNT(*) as Count
        //            FROM [EventLogs]
        //            WHERE 1=1");
        //        if (startDate.HasValue)
        //            hourlyQuery.Append(" AND CreatedAt >= @StartDate");
        //        if (endDate.HasValue)
        //            hourlyQuery.Append(" AND CreatedAt <= @EndDate");
        //        hourlyQuery.Append(" GROUP BY DATEPART(HOUR, CreatedAt)");

        //        statistics.HourlyDistribution = (await connection.QueryAsync(hourlyQuery.ToString(), new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate
        //        })).ToDictionary(row => $"{row.Hour}:00", row => (int)row.Count);

        //        return statistics;
        //    }
        //}

        public async Task<List<CommonEvent>> GetFrequentEventsAsync(
            TimeSpan period,
            string? entityTypeFilter = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var sinceDate = DateTime.UtcNow.Subtract(period);
                var query = new StringBuilder(@"
                    SELECT 
                        EventType,
                        EntityType,
                        COUNT(*) as OccurrenceCount,
                        MAX(CreatedAt) as LastOccurred
                    FROM [EventLogs]
                    WHERE CreatedAt >= @SinceDate");

                if (!string.IsNullOrWhiteSpace(entityTypeFilter))
                    query.Append(" AND EntityType = @EntityTypeFilter");

                query.Append(@"
                    GROUP BY EventType, EntityType
                    ORDER BY COUNT(*) DESC");

                return (await connection.QueryAsync<CommonEvent>(query.ToString(), new
                {
                    SinceDate = sinceDate,
                    EntityTypeFilter = entityTypeFilter
                })).ToList();
            }
        }

        public async Task<int> ArchiveEventsAsync(DateTime cutoffDate)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Copy to archive
                var archiveQuery = @"
                    INSERT INTO [EventLogsArchive]
                    SELECT * FROM [EventLogs]
                    WHERE CreatedAt < @CutoffDate";

                var archivedCount = await connection.ExecuteAsync(archiveQuery, new { CutoffDate = cutoffDate });

                // Delete from main table
                var deleteQuery = @"
                    DELETE FROM [EventLogs]
                    WHERE CreatedAt < @CutoffDate";

                await connection.ExecuteAsync(deleteQuery, new { CutoffDate = cutoffDate });

                return archivedCount;
            }
        }

        public async Task<int> PurgeEventsAsync(DateTime cutoffDate)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    DELETE FROM [EventLogs]
                    WHERE CreatedAt < @CutoffDate";

                return await connection.ExecuteAsync(query, new { CutoffDate = cutoffDate });
            }
        }
    }
}