﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Recipients
{
    public class Recipient
    {
        public int RecipientId { get; set; }
        public int TenantId { get; set; }
        public string ExternalId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DeviceToken { get; set; }
        public string FullName { get; set; }
        public string PreferencesJson { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? UserId { get; set; }

    }
}
