using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.MessageDeliveries.Requests
{
    public class MessageDeliveryCreationRequest
    {
        public long? QueueId { get; set; }
        public Guid RequestId { get; set; }
        public int RecipientId { get; set; }
        public int ProviderId { get; set; }
        public string ChannelType { get; set; }  // "Email", "SMS", "Push"
        public string MessageContentJson { get; set; }
        public string Status { get; set; } = "Queued";
        public int AttemptCount { get; set; } = 0;
        public DateTime? LastAttemptAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string ProviderResponse { get; set; }
        public string ProviderMessageId { get; set; }  // External provider's ID
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
