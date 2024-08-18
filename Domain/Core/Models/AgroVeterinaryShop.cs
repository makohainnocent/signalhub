using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class AgroVeterinaryShop
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public string RegistrationDetails { get; set; }
        public string AuthorizedProducts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
