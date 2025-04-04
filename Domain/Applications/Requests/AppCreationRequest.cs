using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Applications.Requests
{
    public class AppCreationRequest
    {
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int RateLimit { get; set; } = 1000;
        public int OwnerUserId { get; set; }
    }
}
