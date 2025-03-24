using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.InspectionManagement.Requests
{
    public class CreateInspectionRequest
    {
        public int UserId { get; set; }
        public int InspectorId { get; set; }
        public string EntityId { get; set; }
        public string InspectionType { get; set; }
        public DateTime InspectionDate { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public string InspectionReportPdfBase64 { get; set; }

    }
}

