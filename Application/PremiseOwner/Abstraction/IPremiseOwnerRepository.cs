using Domain.Core.Models;
using Domain.PremiseOwner.PremiseOwnerCreateRequest;
using Domain.PremiseOwner.Requests;
using System.Threading.Tasks;
using Application.Common.Abstractions;
using Domain.Common.Responses;

namespace Application.PremiseOwners.Abstraction
{
    public interface IPremiseOwnerRepository
    {
        Task<PremiseOwner> CreatePremiseOwnerAsync(PremiseOwnerCreateRequest request);
        Task<PremiseOwner?> GetPremiseOwnerByIdAsync(int premiseOwnerId);
        Task<PagedResultResponse<PremiseOwner>> GetPremiseOwnersAsync(int pageNumber, int pageSize, string? search = null, int? registerdBy=0);
        Task<PremiseOwner> UpdatePremiseOwnerAsync(int premiseOwnerId, PremiseOwnerUpdateRequest request);
        Task<bool> DeletePremiseOwnerAsync(int premiseOwnerId);
    }
}
