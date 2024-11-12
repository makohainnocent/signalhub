using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class Inspection
    {
        public int InspectionId { get; set; }
        public int UserId { get; set; }
        public string EntityIds { get; set; }
        public string EntityType { get; set; }
        public DateTime InspectionDate { get; set; }
        public string Outcome { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
