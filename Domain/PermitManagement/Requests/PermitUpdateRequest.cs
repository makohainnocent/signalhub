﻿

namespace Domain.PermitManagement.Requests
{
    public class PermitUpdateRequest
    {
        public int PermitId { get; set; } // Primary Key
        public string PermitName { get; set; } // Name of the permit
        public string? Description { get; set; } // Description of the permit (nullable)
        public string? Requirements { get; set; } // Requirements for the permit (nullable)
        public DateTime CreatedAt { get; set; } // Timestamp when the permit was created
        public DateTime? UpdatedAt { get; set; } // Optional last update timestamp (nullable)
    }
}
