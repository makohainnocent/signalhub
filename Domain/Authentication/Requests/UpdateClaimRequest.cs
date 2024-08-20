using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Authentication.Requests
{
    public class UpdateClaimRequest
    {
        public int ClaimId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }
}
