using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Authentication.Responses
{
    public class RoleClaimResponse
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int ClaimId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }

}
