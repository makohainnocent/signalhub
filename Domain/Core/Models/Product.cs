// Domain/Products/Models/Product.cs
namespace Domain.Products.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; }
        public int PremiseId { get; set; }
        public int ManufacturerId { get; set; }
        public int PermitId { get; set; }
        public string RegistrationNumber { get; set; }
        public string ComplianceStatus { get; set; } = "Pending"; // Default status
        public string? ImageBase64 { get; set; } // Base64-encoded image
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}