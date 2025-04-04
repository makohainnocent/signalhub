using Domain.Common.Responses;
using Domain.MessageDeliveries;
using Domain.MessageDeliveries.Requests;

namespace Application.MessageDeliveries.Abstractions
{
    public interface IMessageDeliveriesRepository
    {
        // Delivery Operations
        Task<MessageDelivery> CreateDeliveryAsync(MessageDeliveryCreationRequest request);
        Task<MessageDelivery?> GetDeliveryByIdAsync(int deliveryId);
        Task<PagedResultResponse<MessageDelivery>> GetDeliveriesAsync(
            int pageNumber,
            int pageSize,
            Guid? requestId = null,
            int? recipientId = null,
            int? providerId = null,
            string? channelType = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool? onlyFailed = false);

        // Delivery Processing
        Task<bool> MarkDeliveryAsAttemptedAsync(
            int deliveryId,
            string providerResponse,
            string providerMessageId = null);
        Task<bool> MarkDeliveryAsDeliveredAsync(int deliveryId);
        Task<bool> MarkDeliveryAsFailedAsync(int deliveryId, string failureReason);
        Task<bool> RetryDeliveryAsync(int deliveryId);

        // Status Management
        Task<bool> UpdateDeliveryStatusAsync(int deliveryId, string status);
        Task<int> BulkUpdateDeliveryStatusAsync(IEnumerable<int> deliveryIds, string status);

        // Provider Handling
        Task<bool> UpdateProviderForDeliveryAsync(int deliveryId, int newProviderId);
        Task<PagedResultResponse<MessageDelivery>> GetDeliveriesByProviderAsync(
            int providerId,
            int pageNumber,
            int pageSize,
            string? status = null);

        // Analytics
        //Task<DeliveryAnalytics> GetDeliveryAnalyticsAsync(
        //    DateTime? startDate = null,
        //    DateTime? endDate = null,
        //    int? providerId = null,
        //    string? channelType = null);
        Task<int> CountDeliveriesByStatusAsync(string status);
        Task<Dictionary<string, int>> GetDeliveryStatusDistributionAsync();

        // Maintenance
        Task<int> CleanupOldDeliveriesAsync(DateTime cutoffDate);
        Task<int> RetryFailedDeliveriesAsync(TimeSpan olderThan, int maxAttempts);
    }

    public class DeliveryAnalytics
    {
        public int TotalDeliveries { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public double SuccessRate { get; set; }
        public double AverageDeliveryTimeMs { get; set; }
        public Dictionary<string, int> DeliveryCountByChannel { get; set; } = new();
        public Dictionary<string, int> DeliveryCountByProvider { get; set; } = new();
        public Dictionary<string, double> SuccessRateByHour { get; set; } = new();
    }
}