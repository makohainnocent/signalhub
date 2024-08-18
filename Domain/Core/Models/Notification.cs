using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
