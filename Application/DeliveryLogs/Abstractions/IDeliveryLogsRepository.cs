using Domain.Common.Responses;
using Domain.DeliveryLogs;
using Domain.DeliveryLogs.Requests;

namespace Application.DeliveryLogs.Abstractions
{
    public interface IDeliveryLogsRepository
    {
        // Log Creation
        Task<DeliveryLog> CreateLogAsync(DeliveryLogCreationRequest request);
        Task<int> BulkCreateLogsAsync(IEnumerable<DeliveryLogCreationRequest> logs);

        // Log Retrieval
        Task<DeliveryLog?> GetLogByIdAsync(int logId);
        Task<PagedResultResponse<DeliveryLog>> GetLogsByDeliveryAsync(
            int deliveryId,
            int pageNumber,
            int pageSize,
            string? eventType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        Task<PagedResultResponse<DeliveryLog>> GetLogsByRequestAsync(
            Guid requestId,
            int pageNumber,
            int pageSize,
            string? eventType = null);

        // Event-Specific Queries
        Task<List<DeliveryLog>> GetErrorLogsAsync(int deliveryId);
        Task<List<DeliveryLog>> GetStatusTransitionLogsAsync(int deliveryId);
        Task<DeliveryLog?> GetLastProviderResponseAsync(int deliveryId);

        // Analytics
        //Task<DeliveryTimeline> GetDeliveryTimelineAsync(int deliveryId);
        Task<Dictionary<string, int>> GetEventTypeDistributionAsync(
            int? deliveryId = null);

        Task<List<CommonError>> GetFrequentErrorsAsync(
            TimeSpan period,
            int? providerId = null,
            string? channelType = null);

        // Maintenance
        Task<int> ArchiveLogsAsync(DateTime cutoffDate);
        Task<int> CompressLogDataAsync(TimeSpan olderThan);
    }

    public class DeliveryTimeline
    {
        public DateTime? QueuedTime { get; set; }
        public DateTime? FirstAttempt { get; set; }
        public DateTime? LastAttempt { get; set; }
        public DateTime? DeliveredTime { get; set; }
        public List<DeliveryAttempt> Attempts { get; set; } = new();
        public List<StatusChange> StatusChanges { get; set; } = new();
    }

    public class DeliveryAttempt
    {
        public DateTime AttemptTime { get; set; }
        public string ProviderResponse { get; set; }
        public bool Success { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class StatusChange
    {
        public DateTime ChangeTime { get; set; }
        public string FromStatus { get; set; }
        public string ToStatus { get; set; }
    }

    public class CommonError
    {
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }
        public int OccurrenceCount { get; set; }
        public DateTime LastOccurred { get; set; }
    }
}