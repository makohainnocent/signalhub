﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Livestock
    {
        public int LivestockId { get; set; }
        public int FarmId { get; set; }
        public int UserId { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string HealthStatus { get; set; }
        public string IdentificationMark { get; set; }
    }

}
