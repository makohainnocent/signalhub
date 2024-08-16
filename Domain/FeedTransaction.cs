using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class FeedTransaction
    {
        public int TransactionId { get; set; }
        public int FeedId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Quantity { get; set; }
        public string TransactionType { get; set; } // e.g., Purchase, Sale
        public string Details { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
