using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.FarmManagement.Requests
{
    public class FarmUpdateRequest
    {
        public int FarmId { get; set; } 
        public string FarmName { get; set; } 
        public string Location { get; set; } 
        public decimal Area { get; set; }
        public string FarmImage { get; set; }


    }
}
