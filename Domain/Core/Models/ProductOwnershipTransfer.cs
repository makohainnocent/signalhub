// Domain/ProductOwnershipTransfers/Models/ProductOwnershipTransfer.cs
namespace Domain.ProductOwnershipTransfers.Models
{
    public class ProductOwnershipTransfer
    {
        public int TransferId { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string? ProductDescription { get; set; }
        public int? FromPremiseId { get; set; }
        public string FromPremiseName { get; set; }
        public string FromPremiseAddress { get; set; }
        public int? ToPremiseId { get; set; }
        public string ToPremiseName { get; set; }
        public string ToPremiseAddress { get; set; }
        public bool IsRecipientExternal { get; set; } = false;
        public string Status { get; set; } = "Pending";
        public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? Comments { get; set; }
    }
}