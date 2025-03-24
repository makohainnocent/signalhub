// Domain/ProductOwnershipTransfers/Requests/TransferCreationRequest.cs
namespace Domain.ProductOwnershipTransfers.Requests
{
    public class TransferCreationRequest
    {
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
        public string? Comments { get; set; }
    }
}

