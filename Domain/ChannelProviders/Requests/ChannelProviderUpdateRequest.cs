using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ChannelProviders.Requests
{
    public class ChannelProviderUpdateRequest
    {
        public int ProviderId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string ChannelType { get; set; }  // "Email", "SMS", "Push", etc.
        public string ConfigurationJson { get; set; }
        public bool IsDefault { get; set; } = false;
        public int Priority { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedByUserId { get; set; }
    }
}
