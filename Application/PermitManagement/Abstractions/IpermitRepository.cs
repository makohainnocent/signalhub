using Application.Common.Abstractions;
using Domain.Core.Models;
using Domain.Common.Responses;
using Domain.PermitManagement.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.PermitManagement.Repositories
{
    public interface IPermitRepository
    {
        Task<PermitApplication> CreatePermitApplication(PermitApplicationCreationRequest request);
        Task<PagedResultResponse<PermitApplication>> GetAllPermitApplicationsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? userId = null,
            int? permitId = null,
            string? type = null,
            string? agent = "no");
        Task<PermitApplication?> GetPermitApplicationByIdAsync(int applicationId);
        Task<PermitApplication> UpdatePermitApplication(PermitApplicationUpdateRequest request);
        Task<bool> DeletePermitApplication(int applicationId);
        Task<int> CountPermitApplicationsAsync(int? applicantId = null, int? applicantType = null);
        Task<int> CountPendingApplicationsAsync();
        Task<int> CreatePermitAsync(PermitCreationRequest request);
        Task<Permit?> GetPermitByIdAsync(int permitId);
        Task<Permit> UpdatePermitAsync(PermitUpdateRequest request);
        Task<bool> DeletePermitAsync(int permitId);
        Task<int> CountPermitsAsync(string? permitName = null);

        Task<PagedResultResponse<Permit>> GetAllPermitsAsync(
    int pageNumber,
    int pageSize,
    string? search = null);
    }
}