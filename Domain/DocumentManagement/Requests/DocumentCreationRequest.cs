using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DocumentManagement.Requests
{
    public class DocumentCreationRequest
    {
        public int FarmId { get; set; }
        public int UserId { get; set; }
        public int AnimalId { get; set; }
        public string Type { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
        public string DocumentString { get; set; }
      
    }
}
