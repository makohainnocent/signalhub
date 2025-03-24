using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.InspectionManagement.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.InspectionManagement.Abstractions
{
  
        public interface IInspectionRepository
        {
            Task<Inspection> CreateInspectionAsync(CreateInspectionRequest request);
            Task<Inspection?> GetInspectionByIdAsync(int inspectionId);
        Task<PagedResultResponse<Inspection>> GetAllInspectionsAsync
             (int pageNumber,
            int pageSize,
            string? search = null,
            int? userId = 0,
            int? animalId = 0,
            int? premiseId = 0,
            int? tagId = 0,
            int? productId = 0,
            int? transportId = 0

            );
            Task<Inspection> UpdateInspectionAsync(Inspection inspection);
            Task<bool> DeleteInspectionAsync(int inspectionId);
            Task<int> CountInspectionsAsync(
                int? userId = 0,
                int? animalId = 0,
                int? premiseId = 0,
                int? tagId = 0,
                int? productId = 0,
                int? transportId = 0

            );
            Task<IEnumerable<object>> GetNonCompliantInspectionsThisWeekAsync();
            Task<IEnumerable<object>> GetInspectionsThisWeekAsync();
            Task<IEnumerable<object>> GetCompliantInspectionsThisWeekAsync();
        }
    
}
