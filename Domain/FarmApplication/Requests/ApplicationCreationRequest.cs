using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.FarmApplication.Requests
{
    public class ApplicationCreationRequest
    {
      
        public int FarmId { get; set; }
        public int UserId { get; set; }
        public string RequestDescription { get; set; }
        public string Type { get; set; }
        public string RequestObject { get; set; }
     
    }
}
