using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.FarmManagement.Responses
{
    public class FarmGeofencingWithFarmDetailsResponse
    {
        public int GeofenceId { get; set; }
        public int FarmId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double Radius { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FarmName { get; set; }
        public string Location { get; set; }
    }
}
