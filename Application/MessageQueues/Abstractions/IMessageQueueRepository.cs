using Domain.Common.Responses;
using Domain.MessageQueues.Requests;
using Domain.MessageQueues;

namespace Application.MessageQueue.Abstractions
{
    public interface IMessageQueueRepository
    {
        // Queue Operations
        Task<QueuedMessage> EnqueueMessageAsync(QueuedMessageCreationRequest request);
        Task<QueuedMessage?> DequeueMessageAsync();
        Task<bool> RequeueMessageAsync(long queueId);

        // Message Management
        Task<QueuedMessage?> GetMessageByIdAsync(long queueId);
        Task<PagedResultResponse<QueuedMessage>> GetQueuedMessagesAsync(
            int pageNumber,
            int pageSize,
            Guid? requestId = null,
            int? recipientId = null,
            string? channelType = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool? highPriorityFirst = true);

        // Status Management
        Task<bool> MarkMessageAsProcessingAsync(long queueId);
        Task<bool> MarkMessageAsCompletedAsync(long queueId);
        Task<bool> MarkMessageAsFailedAsync(long queueId, string errorDetails);
        Task<bool> UpdateMessageStatusAsync(long queueId, string status);

        // Priority Handling
        Task<bool> UpdateMessagePriorityAsync(long queueId, int priority);
        Task<bool> PromoteMessagePriorityAsync(long queueId);

        // Bulk Operations
        Task<int> BulkEnqueueMessagesAsync(IEnumerable<QueuedMessageUpdateRequest> messages);
        Task<int> BulkUpdateStatusAsync(IEnumerable<long> queueIds, string status);
        Task<int> RescheduleStaleMessagesAsync(TimeSpan olderThan, string fromStatus);

        // Queue Maintenance
        Task<int> PurgeProcessedMessagesAsync(DateTime olderThan);
        Task<int> CountQueuedMessagesAsync(string? status = null);
        Task<QueueStatus> GetQueueStatusAsync();
    }

    public class QueueStatus
    {
        public int QueuedCount { get; set; }
        public int ProcessingCount { get; set; }
        public int FailedCount { get; set; }
        public Dictionary<string, int> CountByChannelType { get; set; } = new();
        public Dictionary<int, int> CountByPriority { get; set; } = new();
    }
}