using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class ComplianceMonitor
    {
        public int MonitorId { get; set; }
        public int EntityId { get; set; } // Reference to the entity being monitored (e.g., Livestock, FeedBusinessOperator)
        public string EntityType { get; set; } // e.g., Livestock, Feed, Transporter
        public string ComplianceStatus { get; set; }
        public string Penalties { get; set; }
        public DateTime LastInspectionDate { get; set; }
        public DateTime NextInspectionDate { get; set; }
    }
}
