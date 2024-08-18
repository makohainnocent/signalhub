using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class VeterinaryOfficer
    {
        public int OfficerId { get; set; }
        public string Name { get; set; }
        public string ContactDetails { get; set; }
        public string Role { get; set; } // e.g., Inspector, Regulator
    }

}
