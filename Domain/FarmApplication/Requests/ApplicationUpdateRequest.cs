using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.FarmApplication.Requests
{
    public class ApplicationUpdateRequest
    {
        public int ApplicationId { get; set; }

        public int FarmId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string RequestDescription { get; set; }
        public string RequestObject { get; set; }
        public string Status { get; set; }
        public string ResponseObject { get; set; }
        public string ResponseDescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
