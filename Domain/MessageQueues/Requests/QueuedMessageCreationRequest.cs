﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.MessageQueues.Requests
{
    public class QueuedMessageCreationRequest
    {
        public Guid RequestId { get; set; }
        public int RecipientId { get; set; }
        public string ChannelType { get; set; }  // "Email", "SMS", "Push"
        public string MessageContentJson { get; set; }
        public int Priority { get; set; } = 0;
        public string Status { get; set; } = "Queued";
        public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}
