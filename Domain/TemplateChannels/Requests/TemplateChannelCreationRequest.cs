using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.TemplateChannels.Requests
{
    public class TemplateChannelCreationRequest
    {
        public int TemplateId { get; set; }
        public string ChannelType { get; set; }  // e.g., "Email", "SMS", "Push"
        public string ChannelSpecificContentJson { get; set; }
        public bool IsActive { get; set; } = true;
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
