using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.RecipientGroupMembers
{
    public class RecipientGroupMember
    {
        public int GroupId { get; set; }
        public int RecipientId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public int AddedByUserId { get; set; }
    }
}
