using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.NotificationRequests.Requests
{
    public  class NotificationRequestCreationRequest
    {
        public int ApplicationId { get; set; }
        public int TemplateId { get; set; }
        public string RequestDataJson { get; set; }
        public string Priority { get; set; } = "Normal";
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public DateTime? ExpirationAt { get; set; }
        public string CallbackUrl { get; set; }
        public int? RequestedByUserId { get; set; }
    }
}
