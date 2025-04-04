using Domain.Common.Responses;
using Domain.EventLogs.Requests;
using System.Diagnostics;


namespace Application.EventLogs.Abstractions
{
    public interface IEventLogsRepository
    {
        // Core Logging Operations
        Task<EventLog> LogEventAsync(EventLogCreationRequest request);
        Task<int> BulkLogEventsAsync(IEnumerable<EventLogCreationRequest> events);

        // Event Retrieval
        Task<EventLog?> GetEventByIdAsync(int eventId);
        Task<PagedResultResponse<EventLog>> GetEventsAsync(
            int pageNumber,
            int pageSize,
            string? entityType = null,
            string? entityId = null,
            string? eventType = null,
            int? createdByUserId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? searchQuery = null);

        // Entity-Specific Queries
        Task<PagedResultResponse<EventLog>> GetEntityActivityAsync(
            string entityType,
            string entityId,
            int pageNumber,
            int pageSize);

        Task<List<EventLog>> GetEntityTimelineAsync(
            string entityType,
            string entityId,
            int limit = 100);

        // User Activity Tracking
        Task<PagedResultResponse<EventLog>> GetUserActivityAsync(
            int userId,
            int pageNumber,
            int pageSize,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        // System Analytics
        Task<EventStatistics> GetEventStatisticsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<List<CommonEvent>> GetFrequentEventsAsync(
            TimeSpan period,
            string? entityTypeFilter = null);

        // Maintenance
        Task<int> ArchiveEventsAsync(DateTime cutoffDate);
        Task<int> PurgeEventsAsync(DateTime cutoffDate);
    }

    public class EventStatistics
    {
        public int TotalEvents { get; set; }
        public Dictionary<string, int> EventsByType { get; set; } = new();
        public Dictionary<string, int> EventsByEntity { get; set; } = new();
        public Dictionary<string, int> EventsByUser { get; set; } = new();
        public Dictionary<string, int> HourlyDistribution { get; set; } = new();
    }

    public class CommonEvent
    {
        public string EventType { get; set; }
        public string EntityType { get; set; }
        public int OccurrenceCount { get; set; }
        public DateTime LastOccurred { get; set; }
    }

    public static class SystemEvents
    {
        public const string EntityCreated = "EntityCreated";
        public const string EntityUpdated = "EntityUpdated";
        public const string EntityDeleted = "EntityDeleted";
        public const string StatusChanged = "StatusChanged";
        public const string UserLoggedIn = "UserLoggedIn";
        public const string PermissionChanged = "PermissionChanged";
        // Add other system-wide event types as needed
    }

    public static class EntityTypes
    {
        public const string Tenant = "Tenant";
        public const string User = "User";
        public const string Application = "Application";
        public const string Notification = "Notification";
        // Add other entity types as needed
    }
}