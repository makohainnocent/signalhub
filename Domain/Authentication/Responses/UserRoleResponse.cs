using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Authentication.Responses
{
    public class UserRoleResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }

}
