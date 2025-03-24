﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class HealthRecord
    {
        public int HealthRecordId { get; set; }
        public int AnimalId { get; set; }
        public int UserId { get; set; }
        public DateTime DateOfVisit { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public DateTime FollowUpDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
