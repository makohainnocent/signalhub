using Domain.Common.Responses;
using Domain.NotificationRequests;
using Domain.NotificationRequests.Requests;

namespace Application.NotificationRequests.Abstractions
{
    public interface INotificationRequestsRepository
    {
        // Request CRUD Operations
        Task<NotificationRequest> CreateRequestAsync(NotificationRequestCreationRequest request);
        Task<PagedResultResponse<NotificationRequest>> GetRequestsAsync(
            int pageNumber,
            int pageSize,
            int? applicationId = null,
            int? templateId = null,
            string? status = null,
            string? priority = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? requestedByUserId = null);

        Task<NotificationRequest?> GetRequestByIdAsync(Guid requestId);
        Task<int> CountRequestsAsync();

        // Request Processing
        Task<bool> MarkRequestAsProcessingAsync(Guid requestId);
        Task<bool> MarkRequestAsCompletedAsync(Guid requestId);
        Task<bool> MarkRequestAsFailedAsync(Guid requestId, string errorDetails);
        Task<bool> CancelRequestAsync(Guid requestId);

        // Status Management
        Task<bool> UpdateRequestStatusAsync(Guid requestId, string status);
        Task<bool> UpdateRequestPriorityAsync(Guid requestId, string priority);

        // Expiration Handling
        Task<bool> SetRequestExpirationAsync(Guid requestId, DateTime expirationAt);
        Task<List<NotificationRequest>> GetExpiredRequestsAsync();

        // Callback Management
        Task<bool> UpdateCallbackUrlAsync(Guid requestId, string callbackUrl);
        Task<bool> TriggerCallbackAsync(Guid requestId, string callbackData);

        // Request Data Operations
        Task<string> GetRequestDataAsync(Guid requestId);
        Task<bool> UpdateRequestDataAsync(Guid requestId, string requestDataJson);

        // Bulk Operations
        Task<int> BulkUpdateStatusAsync(IEnumerable<Guid> requestIds, string status);
        Task<int> BulkCancelRequestsAsync(IEnumerable<Guid> requestIds);
    }
}