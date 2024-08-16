using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Tag
    {
        public int TagId { get; set; }
        public string TagCode { get; set; }
        public DateTime IssuedDate { get; set; }
        public int IssuedBy { get; set; } // UserId of the issuer
        public int LivestockId { get; set; }
        public string Status { get; set; } // e.g., Active, Inactive, Revoked
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
