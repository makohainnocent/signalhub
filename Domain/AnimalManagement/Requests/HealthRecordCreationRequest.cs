using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.AnimalManagement.Requests
{
    public class HealthRecordCreationRequest
    {
        public int animalId { get; set; }
        public int UserId { get; set; }
        public DateTime DateOfVisit { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public DateTime FollowUpDate { get; set; }
    }
}
