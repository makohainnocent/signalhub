using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Approvals.Requests
{
    public class CreateApprovalRequest
    {
        public int UserId { get; set; }
        public int FarmId { get; set; }                        
            public string LivestockIds { get; set; } = string.Empty; // List of livestock IDs (e.g., comma-separated "1,2,3")
            public string ApprovalDocument { get; set; } = string.Empty; // Base64 encoded string of the approval document
            public string Notes { get; set; } = string.Empty;           // Optional notes or remarks about the approval
        
    }
}
