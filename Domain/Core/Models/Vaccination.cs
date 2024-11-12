using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    using System;

    namespace Domain.Core.Models
    {
        public class Vaccination
        {
            public int VaccinationId { get; set; }                // Unique identifier for the vaccination record
            public int LivestockId { get; set; }
            public int UserId { get; set; }
            public int FarmId { get; set; } // ID of the animal being vaccinated (or user if for humans)
            public string VaccineName { get; set; } = string.Empty; // Name of the vaccine administered
            public string Manufacturer { get; set; } = string.Empty; // Manufacturer of the vaccine
            public DateTime DateAdministered { get; set; }        // Date when the vaccine was administered
            public DateTime? NextDoseDueDate { get; set; }        // Optional date for the next scheduled dose, if any
            public string Dosage { get; set; } = string.Empty;    // Dosage given, e.g., "5 ml"
            public string AdministeredBy { get; set; } = string.Empty; // Name of the veterinarian or medical professional
            public bool IsCompleted { get; set; } = false;        // Indicates if the vaccination course is complete

            // Optional tracking and audit fields
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Record creation timestamp
            public DateTime? UpdatedAt { get; set; }                    // Last update timestamp
            public string Notes { get; set; } = string.Empty;           // Additional notes, if any
        }
    }

}
