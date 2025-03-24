// Application/ProductOwnershipTransfers/Abstractions/IProductOwnershipTransfersRepository.cs
using Domain.Common.Responses;
using Domain.ProductOwnershipTransfers.Models;
using Domain.ProductOwnershipTransfers.Requests;

namespace Application.ProductOwnershipTransfers.Abstractions
{
    public interface IProductOwnershipTransfersRepository
    {
        Task<ProductOwnershipTransfer> CreateTransferAsync(TransferCreationRequest request);
        Task<PagedResultResponse<ProductOwnershipTransfer>> GetTransfersAsync(int pageNumber, int pageSize, int? productId, int? premiseId, string? search = null);
        Task<ProductOwnershipTransfer?> GetTransferByIdAsync(int transferId);
        Task<ProductOwnershipTransfer> UpdateTransferAsync(TransferUpdateRequest request);
        Task<bool> DeleteTransferAsync(int transferId);
        Task<int> CountTransfersAsync();
    }
}