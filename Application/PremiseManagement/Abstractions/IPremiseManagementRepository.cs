using Domain.Authentication.Requests;
using Domain.Authentication.Responses;
using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.PremiseManagement.Requests;


namespace Application.PremiseManagement.Abstractions
{
    public interface IPremiseManagementRepository
    {
        Task<Premise> CreatePremise(PremiseCreationRequest request);
        Task<PagedResultResponse<Premise>> GetAllPremisesAsync(int pageNumber, int pageSize, string? search = null, int? ownerId = null);
        Task<Premise?> GetPremiseByIdAsync(int PremisesId);
        Task<Premise> UpdatePremise(PremiseUpdateRequest request);
        Task<bool> DeletePremise(int PremiseId);
        Task<int> CountPremisesAsync();
        Task<IEnumerable<Premise>> GetPremisesByUserId(int userId,string? agent);


    }
}
