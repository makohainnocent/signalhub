using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.LivestockManagement.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.LivestockManagement.Abstractions
{
    public interface ILivestockManagementRepository
    {
        Task<Livestock> CreateLivestock(LivestockCreationRequest request, int userId);
        Task<PagedResultResponse<Livestock>> GetLivestockByFarmAsync(int farmId, int pageNumber, int pageSize, string? search = null);
        Task<Livestock?> GetLivestockByIdAsync(int livestockId);
        Task<Livestock?> UpdateLivestockAsync(LivestockUpdateRequest request, int userId);
        Task<bool> DeleteLivestockAsync(int livestockId, int userId);
        Task<HealthRecord> CreateHealthRecordAsync(HealthRecordCreationRequest request);
        Task<PagedResultResponse<HealthRecord>> GetHealthRecordsByLivestockIdAsync(int livestockId, int pageNumber, int pageSize, string? search = null);
        Task<HealthRecord?> GetHealthRecordByIdAsync(int healthRecordId);
        Task<bool> UpdateHealthRecordAsync(int healthRecordId, UpdateHealthRecordRequest updateRequest);
        Task<bool> DeleteHealthRecordAsync(int healthRecordId);
        Task<int> CreateDirectiveAsync(CreateDirectiveRequest newDirective);
        Task<PagedResultResponse<Directive>> GetDirectivesByLivestockAsync(int livestockId, int pageNumber, int pageSize, string? search = null);
        Task<Directive?> GetDirectiveByIdAsync(int directiveId);
        Task<bool> UpdateDirectiveDetailsAsync(int directiveId, string directiveDetails);
        Task<bool> DeleteDirectiveAsync(int directiveId);
        Task<int> CountLivestockAsync(int? userId = null, int? farmId = null);

        Task<int> CountHealthRecordsAsync(int? userId = null, int? livestockId = null, int? farmId = null);
    }
}
