using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.FarmManagement.Requests
{
    public class FarmGeofencingRequest
    {
        public int FarmId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double Radius { get; set; } 
    }
}
