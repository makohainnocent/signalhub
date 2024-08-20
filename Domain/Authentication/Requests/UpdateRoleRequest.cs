using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Authentication.Requests
{
    public class UpdateRoleRequest
    {   
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
