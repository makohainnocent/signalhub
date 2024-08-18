using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class Permit
    {
        public int PermitId { get; set; }
        public int LivestockId { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string PermitType { get; set; } // e.g., Transport, Export, Import
        public string Status { get; set; }
        public string Details { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
