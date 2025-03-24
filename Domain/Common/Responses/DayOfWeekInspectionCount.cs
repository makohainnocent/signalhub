using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Responses
{
    public class DayOfWeekInspectionCount
    {
        public int DayOfWeek { get; set; } // 1 = Sunday, 2 = Monday, etc. (SQL Server default)
        public int InspectionCount { get; set; }
    }
}
