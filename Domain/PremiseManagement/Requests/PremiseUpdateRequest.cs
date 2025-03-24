using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PremiseManagement.Requests
{
    public class PremiseUpdateRequest
    {
        public int PremisesId { get; set; } // Previously FarmId
        public string Name { get; set; } // Previously FarmName
        public string Coordinates { get; set; } // New field for CPS Coordinates
        public string Type { get; set; } // New field for Farm Type (e.g., poultry, beef, pigs, etc.)
        public int OwnerId { get; set; } // Previously UserId
        public string Status { get; set; } = "Pending"; // Default value
        public string PremiseImage { get; set; } // New fields based on provided details
        public string Province { get; set; } // Province of the premise
        public string DistrictConstituency { get; set; } // District and Constituency
        public string Ward { get; set; } // Ward of the premise
        public string VillageLocalityAddress { get; set; } // Address of the Village or Locality
        public string Chiefdom { get; set; } // Chiefdom of the premise
        public string Headman { get; set; } // Name of the Headman
        public string VeterinaryCamp { get; set; } // Name of the Veterinary Camp
        public string CampOfficerNames { get; set; } // Names of the Camp Officers
        public string VeterinaryOfficerNames { get; set; } // Names of the Veterinary or Livestock Officers
        public string PhysicalPostalAddress { get; set; } // Physical or Postal Address
        public string HandlingFacility { get; set; } // Type of handling facility (e.g., crush-pen, dip-tank, livestock service centre)
        public string AlternativeAddresses { get; set; } // Alternative addresses for the premise
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow; // New field for registration date
        public DateTime? UpdatedAt { get; set; }

    }
}
