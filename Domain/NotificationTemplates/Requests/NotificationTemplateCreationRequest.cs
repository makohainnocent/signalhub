using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.NotificationTemplates.Requests
{
    public class NotificationTemplateCreationRequest
    {
        public int ApplicationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string VariablesSchemaJson { get; set; }
        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public int? ApprovedByUserId { get; set; }
        public string ApprovalStatus { get; set; } = "Draft";
    }
}
