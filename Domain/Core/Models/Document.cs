using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public  class Document
    {
        public int DocumentId { get; set; }
        public int? FarmId { get; set; }
        public int? UserId { get; set; }
        public int? AnimalId { get; set; }
        public string? Type { get; set; }
        public string? Owner { get; set; }
        public string?Description { get; set; }
        public string? DocumentString { get; set; } //base64 encoded string
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
