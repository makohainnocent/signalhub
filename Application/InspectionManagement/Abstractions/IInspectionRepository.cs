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
            Task<PagedResultResponse<Inspection>> GetAllInspectionsAsync(int pageNumber, int pageSize, string? search = null, int? userId = null);
            Task<Inspection> UpdateInspectionAsync(Inspection inspection);
            Task<bool> DeleteInspectionAsync(int inspectionId);
            Task<int> CountInspectionsAsync(int? userId = null, int? livestockId = null, int? farmId = null);
        }
    
}
