﻿using System;

namespace Domain.Core.Models
{
    public class Animal
    {
        public int AnimalId { get; set; } // Primary key, auto-incrementing
        public string Species { get; set; } // Species of the animal (required)
        public string? Breed { get; set; } // Breed of the animal (optional)
        public string  BirthDate { get; set; } // Age of the animal (optional)
        public string Color { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string HealthStatus { get; set; } // Health status of the animal (required)
        public string IdentificationMark { get; set; } // Unique identification mark (required)
        public int OwnerId { get; set; } // Foreign key to Users table (owner)
        public int PremisesId { get; set; } // Foreign key to Premises table
        public string Status { get; set; } = "Alive"; // Status of the animal, default is "Alive"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp when the animal was registered (default to current UTC time)
        public DateTime? UpdatedAt { get; set; } // Optional last update timestamp
        public string? AnimalImage { get; set; } // Base64 encoded image of the animal (optional)
    }
}