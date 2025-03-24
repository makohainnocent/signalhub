namespace Domain.Core.Models
{
    public class TagApplication
    {
        public int ApplicationId { get; set; }
        public string ApplicantType { get; set; } // Farmer, Farm, Other
        public int ApplicantId { get; set; } // ID of the applicant
        public int NumberOfTags { get; set; }
        public string Purpose { get; set; } // Optional
        public string Status { get; set; } // Pending, Approved, Rejected
        public int AppliedBy { get; set; } // User ID
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public int? ReviewedBy { get; set; } // User ID (optional)
        public DateTime? ReviewedAt { get; set; } // Optional
        public string Comments { get; set; } // Optional
    }
}