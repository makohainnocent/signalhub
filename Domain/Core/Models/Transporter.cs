using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class Transporter
    {
        public int TransporterId { get; set; }
        public string Name { get; set; }
        public string ContactDetails { get; set; }
        public string VehicleDetails { get; set; }
        public string ComplianceStatus { get; set; }
    }

}
