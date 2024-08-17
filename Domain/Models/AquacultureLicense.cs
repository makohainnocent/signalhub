using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class AquacultureLicense
    {
        public int LicenseId { get; set; }
        public string FacilityName { get; set; }
        public string WaterUsePermitDetails { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string EnvironmentalImpactDetails { get; set; }
        public string ChemicalRestrictions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
