﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.RecipientGroups.Requests
{
    public class RecipientGroupUpdateRequest
    {
        public int GroupId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
