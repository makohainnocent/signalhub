﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DeliveryLogs
{
    public class DeliveryLog
    {
        public int LogId { get; set; }
        public int DeliveryId { get; set; }
        public string EventType { get; set; }
        public string EventDataJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
