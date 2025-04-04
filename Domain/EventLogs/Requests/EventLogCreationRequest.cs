using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.EventLogs.Requests
{
    public class EventLogCreationRequest
    {
        public string EntityType { get; set; }  
        public string EntityId { get; set; }    
        public string EventType { get; set; }   
        public string EventDataJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }
    }
}
