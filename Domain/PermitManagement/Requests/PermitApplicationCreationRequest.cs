using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PermitManagement.Requests
{
    public class PermitApplicationCreationRequest
    {
        public int ApplicationId { get; set; } // Primary Key
        public string ApplicantType { get; set; } // Type of applicant (e.g., Farmer, Farm, Transporter)
        public int PermitId { get; set; }
        public int AgentId { get; set; }
        public int ApplicantId { get; set; } // ID of the applicant
        public string Documents { get; set; } // Base64-encoded PDF of required documents
        public string Status { get; set; } // Status of the application (e.g., Pending, Approved, Rejected, Revoked)
        public int AppliedBy { get; set; } // ID of the user who applied
        public DateTime AppliedAt { get; set; } // Timestamp when the application was submitted
        
    }
}
