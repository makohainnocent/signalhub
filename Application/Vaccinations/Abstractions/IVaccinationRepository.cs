using Domain.Common.Responses;
using Domain.Core.Models.Domain.Core.Models;
using Domain.Vaccinations.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Vaccinations.Abstractions
{
    public interface IVaccinationRepository
    {
        Task<Vaccination> CreateVaccinationAsync(CreateVaccinationRequest request);
        Task<Vaccination?> GetVaccinationByIdAsync(int vaccinationId);
        Task<PagedResultResponse<Vaccination>> GetAllVaccinationsAsync(int pageNumber, int pageSize,int? farmId = null, int? userId = null, string? search = null,int? livestockId = null);
        Task<Vaccination> UpdateVaccinationAsync(Vaccination vaccination);
        Task<bool> DeleteVaccinationAsync(int vaccinationId);
        Task<int> CountVaccinationsAsync(int? userId = null, int? farmId = null, int? livestockId = null);
    }
}
