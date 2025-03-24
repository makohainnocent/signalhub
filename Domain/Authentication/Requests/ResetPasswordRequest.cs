using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Authentication.Requests
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }
}
