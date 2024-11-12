using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Vaccinations.Requests
{
    public class CreateVaccinationRequest
    {
        public int LivestockId { get; set; }
        public int UserId { get; set; }     // ID of the livestock being vaccinated
        public int FarmId { get; set; }                      // ID of the farm where the vaccination takes place
        public string VaccineName { get; set; } = string.Empty; // Name of the vaccine administered
        public string Manufacturer { get; set; } = string.Empty; // Manufacturer of the vaccine
        public DateTime DateAdministered { get; set; }       // Date when the vaccine was administered
        public DateTime? NextDoseDueDate { get; set; }       // Optional next scheduled dose date
        public string Dosage { get; set; } = string.Empty;   // Dosage given, e.g., "5 ml"
        public string AdministeredBy { get; set; } = string.Empty; // Name of the veterinarian or medical professional
        public string Notes { get; set; } = string.Empty;    // Additional notes for the vaccination
    }
}
