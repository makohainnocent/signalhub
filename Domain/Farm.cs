using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Farm
    {
        public int FarmId { get; set; }
        public int UserId { get; set; }
        public string FarmName { get; set; }
        public string Location { get; set; }
        public decimal Area { get; set; } // in acres
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
