using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class Feed
    {
        public int FeedId { get; set; }
        public string FeedName { get; set; }
        public string FeedType { get; set; }
        public DateTime ManufactureDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Supplier { get; set; }
        public string Details { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
