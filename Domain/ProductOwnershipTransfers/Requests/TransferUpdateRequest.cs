// Domain/ProductOwnershipTransfers/Requests/TransferUpdateRequest.cs
namespace Domain.ProductOwnershipTransfers.Requests
{
    public class TransferUpdateRequest
    {
        public int TransferId { get; set; }
        public string Status { get; set; }
        public string? Comments { get; set; }
    }
}