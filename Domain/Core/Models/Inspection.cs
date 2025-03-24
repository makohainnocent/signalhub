using System;

namespace Domain.Core.Models
{
    public class Inspection
    {
        public int InspectionId { get; set; } // Auto-incrementing primary key
        public int InspectorId { get; set; } // Replaces UserId to match "InspectorId"
        public int EntityId { get; set; } // Entity being inspected
        public string InspectionType { get; set; } // Type of entity being inspected (e.g., Premise, Product)
        public DateTime InspectionDate { get; set; } // Date of inspection
        public string Status { get; set; } // Inspection status (e.g., Pass, Fail, Pending)
        public string Comments { get; set; } // Optional comments from the inspector
        public string InspectionReportPdfBase64 { get; set; } // Base64-encoded PDF of the inspection report
        public DateTime CreatedAt { get; set; } // Timestamp when the inspection was created
        public DateTime? UpdatedAt { get; set; } // Optional last update timestamp
    }
}
