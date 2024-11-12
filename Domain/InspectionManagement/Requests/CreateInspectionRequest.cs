using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.InspectionManagement.Requests
{
    public class CreateInspectionRequest
    {
        public int UserId { get; set; }
        public string EntityIds { get; set; }
        public string EntityType { get; set; }
        public DateTime InspectionDate { get; set; }
        public string Outcome { get; set; }
        public string Notes { get; set; }
      
    }
}
