using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class FeedBusinessOperator
    {
        public int OperatorId { get; set; }
        public string BusinessName { get; set; }
        public string RegistrationDetails { get; set; }
        public string ComplianceStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
