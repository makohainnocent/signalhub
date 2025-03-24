namespace Domain.Products.Requests
{
    public class ProductUpdateRequest
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public int? PremiseId { get; set; }
        public int? ManufacturerId { get; set; }
        public int? PermitId { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? ComplianceStatus { get; set; }
        public string? ImageBase64 { get; set; }
    }
}