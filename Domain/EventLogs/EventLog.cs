using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.EventLogs
{
    public class EventLog
    {
        public int EventId { get; set; }
        public string EntityType { get; set; }  // e.g., "Tenant", "Application", "Recipient"
        public string EntityId { get; set; }    // String to support both GUID and integer IDs
        public string EventType { get; set; }   // e.g., "Created", "Updated", "Deleted"
        public string EventDataJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }
    }
}
