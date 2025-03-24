using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.AnimalManagement.Requests
{
    public class UpdateHealthRecordRequest
    {
        public int? animalId { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public DateTime? DateOfVisit { get; set; }
    }

}
