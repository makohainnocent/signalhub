using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Directive
    {
        public int DirectiveId { get; set; }
        public int LivestockId { get; set; }
        public DateTime DirectiveDate { get; set; }
        public string DirectiveDetails { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
