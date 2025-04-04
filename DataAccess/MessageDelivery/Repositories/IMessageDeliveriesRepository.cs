// DataAccess/MessageDeliveries/Repositories/MessageDeliveriesRepository.cs
using Application.MessageDeliveries.Abstractions;
using Domain.MessageDeliveries;
using Domain.MessageDeliveries.Requests;
using Dapper;
using System.Data;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Text;
using System.Text.Json;

namespace DataAccess.MessageDeliveries.Repositories
{
    public class MessageDeliveriesRepository : IMessageDeliveriesRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public MessageDeliveriesRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<MessageDelivery> CreateDeliveryAsync(MessageDeliveryCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO [MessageDeliveries] (
                        QueueId,
                        RequestId,
                        RecipientId,
                        ProviderId,
                        ChannelType,
                        MessageContentJson,
                        Status,
                        CreatedAt
                    )
                    VALUES (
                        @QueueId,
                        @RequestId,
                        @RecipientId,
                        @ProviderId,
                        @ChannelType,
                        @MessageContentJson,
                        @Status,
                        @CreatedAt
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    request.QueueId,
                    request.RequestId,
                    request.RecipientId,
                    request.ProviderId,
                    request.ChannelType,
                    MessageContentJson = JsonSerializer.Serialize(request.MessageContentJson),
                    Status = "Queued",
                    CreatedAt = DateTime.UtcNow
                };

                var deliveryId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new MessageDelivery
                {
                    DeliveryId = deliveryId,
                    QueueId = request.QueueId,
                    RequestId = request.RequestId,
                    RecipientId = request.RecipientId,
                    ProviderId = request.ProviderId,
                    ChannelType = request.ChannelType,
                    MessageContentJson = parameters.MessageContentJson,
                    Status = "Queued",
                    CreatedAt = parameters.CreatedAt
                };
            }
        }

        public async Task<MessageDelivery?> GetDeliveryByIdAsync(int deliveryId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [MessageDeliveries]
                    WHERE DeliveryId = @DeliveryId";

                return await connection.QuerySingleOrDefaultAsync<MessageDelivery>(query, new { DeliveryId = deliveryId });
            }
        }

        public async Task<PagedResultResponse<MessageDelivery>> GetDeliveriesAsync(
            int pageNumber,
            int pageSize,
            Guid? requestId = null,
            int? recipientId = null,
            int? providerId = null,
            string? channelType = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool? onlyFailed = false)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = new StringBuilder(@"
                    SELECT *
                    FROM [MessageDeliveries]
                    WHERE 1=1");

                if (requestId.HasValue)
                    query.Append(" AND RequestId = @RequestId");

                if (recipientId.HasValue)
                    query.Append(" AND RecipientId = @RecipientId");

                if (providerId.HasValue)
                    query.Append(" AND ProviderId = @ProviderId");

                if (!string.IsNullOrWhiteSpace(channelType))
                    query.Append(" AND ChannelType = @ChannelType");

                if (!string.IsNullOrWhiteSpace(status))
                    query.Append(" AND Status = @Status");

                if (fromDate.HasValue)
                    query.Append(" AND CreatedAt >= @FromDate");

                if (toDate.HasValue)
                    query.Append(" AND CreatedAt <= @ToDate");

                if (onlyFailed == true)
                    query.Append(" AND Status = 'Failed'");

                query.Append(@"
                    ORDER BY CreatedAt DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [MessageDeliveries]
                    WHERE 1=1");

                // Repeat filters for count query
                if (requestId.HasValue)
                    query.Append(" AND RequestId = @RequestId");

                if (recipientId.HasValue)
                    query.Append(" AND RecipientId = @RecipientId");

                if (providerId.HasValue)
                    query.Append(" AND ProviderId = @ProviderId");

                if (!string.IsNullOrWhiteSpace(channelType))
                    query.Append(" AND ChannelType = @ChannelType");

                if (!string.IsNullOrWhiteSpace(status))
                    query.Append(" AND Status = @Status");

                if (fromDate.HasValue)
                    query.Append(" AND CreatedAt >= @FromDate");

                if (toDate.HasValue)
                    query.Append(" AND CreatedAt <= @ToDate");

                if (onlyFailed == true)
                    query.Append(" AND Status = 'Failed'");

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    RequestId = requestId,
                    RecipientId = recipientId,
                    ProviderId = providerId,
                    ChannelType = channelType,
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate
                }))
                {
                    var deliveries = multi.Read<MessageDelivery>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<MessageDelivery>
                    {
                        Items = deliveries,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<bool> MarkDeliveryAsAttemptedAsync(
            int deliveryId,
            string providerResponse,
            string providerMessageId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageDeliveries]
                    SET 
                        Status = 'Attempted',
                        AttemptCount = AttemptCount + 1,
                        LastAttemptAt = GETUTCDATE(),
                        ProviderResponse = @ProviderResponse,
                        ProviderMessageId = @ProviderMessageId
                    WHERE DeliveryId = @DeliveryId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    DeliveryId = deliveryId,
                    ProviderResponse = providerResponse,
                    ProviderMessageId = providerMessageId
                });

                return affectedRows > 0;
            }
        }

        public async Task<bool> MarkDeliveryAsDeliveredAsync(int deliveryId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageDeliveries]
                    SET 
                        Status = 'Delivered',
                        DeliveredAt = GETUTCDATE()
                    WHERE DeliveryId = @DeliveryId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    DeliveryId = deliveryId
                });

                return affectedRows > 0;
            }
        }

        public async Task<bool> MarkDeliveryAsFailedAsync(int deliveryId, string failureReason)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageDeliveries]
                    SET 
                        Status = 'Failed',
                        ProviderResponse = @FailureReason
                    WHERE DeliveryId = @DeliveryId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    DeliveryId = deliveryId,
                    FailureReason = failureReason
                });

                return affectedRows > 0;
            }
        }

        public async Task<bool> RetryDeliveryAsync(int deliveryId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageDeliveries]
                    SET 
                        Status = 'Queued',
                        AttemptCount = AttemptCount + 1
                    WHERE DeliveryId = @DeliveryId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    DeliveryId = deliveryId
                });

                return affectedRows > 0;
            }
        }

        public async Task<bool> UpdateDeliveryStatusAsync(int deliveryId, string status)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageDeliveries]
                    SET Status = @Status
                    WHERE DeliveryId = @DeliveryId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    DeliveryId = deliveryId,
                    Status = status
                });

                return affectedRows > 0;
            }
        }

        public async Task<int> BulkUpdateDeliveryStatusAsync(IEnumerable<int> deliveryIds, string status)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageDeliveries]
                    SET Status = @Status
                    WHERE DeliveryId IN @DeliveryIds";

                return await connection.ExecuteAsync(query, new
                {
                    DeliveryIds = deliveryIds,
                    Status = status
                });
            }
        }

        public async Task<bool> UpdateProviderForDeliveryAsync(int deliveryId, int newProviderId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [MessageDeliveries]
                    SET 
                        ProviderId = @NewProviderId,
                        Status = 'Queued'
                    WHERE DeliveryId = @DeliveryId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    DeliveryId = deliveryId,
                    NewProviderId = newProviderId
                });

                return affectedRows > 0;
            }
        }

        public async Task<PagedResultResponse<MessageDelivery>> GetDeliveriesByProviderAsync(
            int providerId,
            int pageNumber,
            int pageSize,
            string? status = null)
        {
            return await GetDeliveriesAsync(
                pageNumber,
                pageSize,
                providerId: providerId,
                status: status);
        }

        //public async Task<DeliveryAnalytics> GetDeliveryAnalyticsAsync(
        //    DateTime? startDate = null,
        //    DateTime? endDate = null,
        //    int? providerId = null,
        //    string? channelType = null)
        //{
        //    using (var connection = _dbConnectionProvider.CreateConnection())
        //    {
        //        connection.Open();

        //        var analytics = new DeliveryAnalytics();

        //        // Base where clause
        //        var whereClause = new StringBuilder("WHERE 1=1");
        //        if (startDate.HasValue)
        //            whereClause.Append(" AND CreatedAt >= @StartDate");
        //        if (endDate.HasValue)
        //            whereClause.Append(" AND CreatedAt <= @EndDate");
        //        if (providerId.HasValue)
        //            whereClause.Append(" AND ProviderId = @ProviderId");
        //        if (!string.IsNullOrWhiteSpace(channelType))
        //            whereClause.Append(" AND ChannelType = @ChannelType");

        //        // Total deliveries
        //        var totalQuery = $"SELECT COUNT(*) FROM [MessageDeliveries] {whereClause}";
        //        analytics.TotalDeliveries = await connection.ExecuteScalarAsync<int>(totalQuery, new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate,
        //            ProviderId = providerId,
        //            ChannelType = channelType
        //        });

        //        // Successful deliveries
        //        var successQuery = $"SELECT COUNT(*) FROM [MessageDeliveries] {whereClause} AND Status = 'Delivered'";
        //        analytics.SuccessfulDeliveries = await connection.ExecuteScalarAsync<int>(successQuery, new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate,
        //            ProviderId = providerId,
        //            ChannelType = channelType
        //        });

        //        // Failed deliveries
        //        var failedQuery = $"SELECT COUNT(*) FROM [MessageDeliveries] {whereClause} AND Status = 'Failed'";
        //        analytics.FailedDeliveries = await connection.ExecuteScalarAsync<int>(failedQuery, new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate,
        //            ProviderId = providerId,
        //            ChannelType = channelType
        //        });

        //        // Success rate
        //        analytics.SuccessRate = analytics.TotalDeliveries > 0
        //            ? (double)analytics.SuccessfulDeliveries / analytics.TotalDeliveries * 100
        //            : 0;

        //        // Average delivery time
        //        var avgTimeQuery = $@"
        //            SELECT AVG(DATEDIFF(MILLISECOND, CreatedAt, DeliveredAt))
        //            FROM [MessageDeliveries]
        //            {whereClause} AND Status = 'Delivered' AND DeliveredAt IS NOT NULL";

        //        analytics.AverageDeliveryTimeMs = await connection.ExecuteScalarAsync<double>(avgTimeQuery, new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate,
        //            ProviderId = providerId,
        //            ChannelType = channelType
        //        });

        //        // Delivery count by channel
        //        var channelQuery = $@"
        //            SELECT ChannelType, COUNT(*) as Count
        //            FROM [MessageDeliveries]
        //            {whereClause}
        //            GROUP BY ChannelType";

        //        analytics.DeliveryCountByChannel = (await connection.QueryAsync(channelQuery, new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate,
        //            ProviderId = providerId,
        //            ChannelType = channelType
        //        })).ToDictionary(row => (string)row.ChannelType, row => (int)row.Count);

        //        // Delivery count by provider
        //        var providerQuery = $@"
        //            SELECT ProviderId, COUNT(*) as Count
        //            FROM [MessageDeliveries]
        //            {whereClause}
        //            GROUP BY ProviderId";

        //        analytics.DeliveryCountByProvider = (await connection.QueryAsync(providerQuery, new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate,
        //            ProviderId = providerId,
        //            ChannelType = channelType
        //        })).ToDictionary(row => row.ProviderId.ToString(), row => (int)row.Count);

        //        // Success rate by hour
        //        var hourQuery = $@"
        //            SELECT 
        //                DATEPART(HOUR, CreatedAt) as Hour,
        //                COUNT(*) as Total,
        //                SUM(CASE WHEN Status = 'Delivered' THEN 1 ELSE 0 END) as SuccessCount
        //            FROM [MessageDeliveries]
        //            {whereClause}
        //            GROUP BY DATEPART(HOUR, CreatedAt)";

        //        var hourlyResults = await connection.QueryAsync(hourQuery, new
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate,
        //            ProviderId = providerId,
        //            ChannelType = channelType
        //        });

        //        analytics.SuccessRateByHour = hourlyResults.ToDictionary(
        //            row => $"{row.Hour}:00",
        //            row => row.Total > 0 ? (double)row.SuccessCount / row.Total * 100 : 0
        //        );

        //        return analytics;
        //    }
        //}

        public async Task<int> CountDeliveriesByStatusAsync(string status)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT COUNT(*)
                    FROM [MessageDeliveries]
                    WHERE Status = @Status";

                return await connection.ExecuteScalarAsync<int>(query, new { Status = status });
            }
        }

        public async Task<Dictionary<string, int>> GetDeliveryStatusDistributionAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT Status, COUNT(*) as Count
                    FROM [MessageDeliveries]
                    GROUP BY Status";

                return (await connection.QueryAsync(query))
                    .ToDictionary(row => (string)row.Status, row => (int)row.Count);
            }
        }

        public async Task<int> CleanupOldDeliveriesAsync(DateTime cutoffDate)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    DELETE FROM [MessageDeliveries]
                    WHERE CreatedAt < @CutoffDate
                    AND Status IN ('Delivered', 'Failed')";

                return await connection.ExecuteAsync(query, new { CutoffDate = cutoffDate });
            }
        }

        public async Task<int> RetryFailedDeliveriesAsync(TimeSpan olderThan, int maxAttempts)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var cutoffDate = DateTime.UtcNow.Subtract(olderThan);

                var query = @"
                    UPDATE [MessageDeliveries]
                    SET 
                        Status = 'Queued',
                        AttemptCount = AttemptCount + 1
                    WHERE Status = 'Failed'
                    AND CreatedAt < @CutoffDate
                    AND AttemptCount < @MaxAttempts";

                return await connection.ExecuteAsync(query, new
                {
                    CutoffDate = cutoffDate,
                    MaxAttempts = maxAttempts
                });
            }
        }
    }
}