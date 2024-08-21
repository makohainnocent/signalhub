using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Authentication.Requests
{
    public class RemoveClaimFromUserRequest
    {
        public int UserId { get; set; }
        public int ClaimId { get; set; }
    }

}
