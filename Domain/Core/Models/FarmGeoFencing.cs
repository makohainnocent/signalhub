using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class FarmGeofencing
    {
        public int GeofenceId { get; set; }
        public int FarmId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double Radius { get; set; } // in meters
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
