// Domain/Transportation/Models/Transportation.cs
namespace Domain.Transportation.Models
{
    public class Transportation
    {
        public int TransportId { get; set; }
        public int PermitId { get; set; }
        public int UserId { get; set; }
        public int SourcePremisesId { get; set; }
        public string SourceAddress { get; set; }
        public int DestinationPremisesId { get; set; }
        public string DestinationAddress { get; set; }
        public int TransporterId { get; set; }
        public string VehicleDetails { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ItemsDocument { get; set; } // Base64-encoded document
        public string ReasonForTransport { get; set; }
        public string? Description { get; set; }
        public int AgentId { get; set; }
        public int VetId { get; set; }
        public string Status { get; set; } = "Pending"; // Default status
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

    }
}