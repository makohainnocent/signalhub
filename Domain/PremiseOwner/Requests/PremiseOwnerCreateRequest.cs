using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PremiseOwner.PremiseOwnerCreateRequest
{
    public class PremiseOwnerCreateRequest
    {
        public int  RegisterdById { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string VillageOrAddress { get; set; }


        public string Names { get; set; }
        public string Surname { get; set; }
        public string OtherNames { get; set; }
        public string Sex { get; set; }
        public string NRC { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public int AgentId { get; set; }


        public string ArtificialPersonName { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonID { get; set; }
        public string ContactPersonPhoneNumber { get; set; }
        public string ContactPersonEmail { get; set; }
    }
}
