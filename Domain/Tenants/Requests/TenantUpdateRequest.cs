using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Tenants.Requests
{
    public class TenantUpdateRequest
    {
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public int OwnerUserId { get; set; }
    }
}
