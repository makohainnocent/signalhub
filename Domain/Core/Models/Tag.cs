namespace Domain.Core.Models
{
    public class Tag
    {
        public int TagId { get; set; }
        public string TagNumber { get; set; } // Unique tag number
        public string TagType { get; set; } // RFID, Ear Tag, Microchip
        public string Manufacturer { get; set; }
        public string BatchNumber { get; set; } // Optional
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}