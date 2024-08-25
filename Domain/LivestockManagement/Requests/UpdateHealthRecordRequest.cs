using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.LivestockManagement.Requests
{
    public class UpdateHealthRecordRequest
    {
        public int? LivestockId { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public DateTime? DateOfVisit { get; set; }
    }

}
