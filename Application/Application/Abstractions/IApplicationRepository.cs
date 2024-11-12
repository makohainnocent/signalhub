using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.FarmApplication.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Application.Abstractions
{
    public interface IApplicationRepository
    {
        Task<FarmApplicationModel> CreateApplication(ApplicationCreationRequest request);
        Task<PagedResultResponse<FarmApplicationModel>> GetAllApplicationsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? userId = null,
            int? farmId = null,
            string? type = null);
        Task<FarmApplicationModel?> GetApplicationByIdAsync(int applicationId);
        Task<FarmApplicationModel> UpdateApplication(ApplicationUpdateRequest request);
        Task<bool> DeleteApplication(int applicationId);
        Task<int> CountApplicationsAsync(int? userId = null, int? farmId = null);
    }
}
