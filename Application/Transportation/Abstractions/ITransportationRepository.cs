// Application/Transportation/Abstractions/ITransportationRepository.cs
using Domain.Common.Responses;
using Domain.Transportation.Requests;
using Domain.Transportation.Models;

namespace Application.Transportations.Abstractions
{
    public interface ITransportationRepository
    {
        Task<Transportation> CreateTransportationAsync(TransportationCreationRequest request);
        Task<PagedResultResponse<Transportation>> GetTransportationsAsync(int pageNumber, int pageSize, string? search = null, int? userId = null, int? frompremiseId = null,string? agent="no", string? vet = "no");
        Task<Transportation?> GetTransportationByIdAsync(int transportIdz);
        Task<Transportation> UpdateTransportationAsync(TransportationUpdateRequest request);
        Task<bool> DeleteTransportationAsync(int transportId);
        Task<int> CountTransportationsAsync();
    }
}