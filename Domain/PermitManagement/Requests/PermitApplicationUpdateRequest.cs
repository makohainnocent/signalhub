using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PermitManagement.Requests
{
    public class PermitApplicationUpdateRequest
    {
        public int ApplicationId { get; set; } // Primary Key
        public int PermitId { get; set; } // Permit ID
        public string ApplicantType { get; set; } // Type of applicant (e.g., Farmer, Farm, Transporter)
        public int ApplicantId { get; set; } // ID of the applicant
        public string Documents { get; set; } // Base64-encoded PDF of required documents
        public string Status { get; set; } // Status of the application (e.g., Pending, Approved, Rejected, Revoked)
        public int AppliedBy { get; set; } // ID of the user who applied
        public DateTime AppliedAt { get; set; } // Timestamp when the application was submitted
        public int? ReviewedBy { get; set; } // ID of the reviewer (nullable)
        public DateTime? ReviewedAt { get; set; } // Timestamp when the application was reviewed (nullable)
        public DateTime? IssuedAt { get; set; } // Timestamp when the permit was issued (nullable)
        public DateTime? ExpiryDate { get; set; } // Expiry date of the permit (nullable)
        public string PermitPdf { get; set; } // Base64-encoded PDF of the issued permit (nullable)
        public DateTime? RevokedAt { get; set; } // Timestamp when the permit was revoked (nullable)
        public int? RevokedBy { get; set; } // ID of the user who revoked the permit (nullable)
        public string Comments { get; set; } // Comments or notes from the reviewer (nullable)

    }
}
