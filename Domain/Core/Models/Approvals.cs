using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class Approval
    {
        public int ApprovalId { get; set; }                     // Unique identifier for the approval record
        public int UserId { get; set; }                         // ID of the user requesting or approving
        public int FarmId { get; set; }                         // ID of the farm associated with the approval
        public string LivestockIds { get; set; } = string.Empty; // List of livestock IDs (e.g., comma-separated "1,2,3")

        public string ApprovalDocument { get; set; } = string.Empty; // Base64 encoded string of the approval document
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Timestamp when the approval was created
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; } = string.Empty;           // Optional notes or remarks about the approval
    }

}
