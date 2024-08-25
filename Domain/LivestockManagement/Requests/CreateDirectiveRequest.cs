using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.LivestockManagement.Requests
{
    public class CreateDirectiveRequest
    {
        public int LivestockId { get; set; }
        public DateTime DirectiveDate { get; set; }
        public string DirectiveDetails { get; set; }
    }
}
