namespace Domain.Core.Models
{
    public class TagIssuance
    {
        public int IssuanceId { get; set; }
        public int ApplicationId { get; set; } // Foreign key to TagApplications table
        public string IssuedToType { get; set; } // Farmer, Farm, Other
        public int IssuedToId { get; set; } // ID of the entity the tag is issued to
        public int IssuedBy { get; set; } // User ID
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; } // Optional
        public string Status { get; set; } = "Approved";// Active, Revoked, Lost, Damaged
        public DateTime? RevokedAt { get; set; } // Optional
        public int? RevokedBy { get; set; } // Optional
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } // Optional
        public int TagId { get; set; }

        // Add a property for the Tag details
        public Tag Tag { get; set; }


    }
}