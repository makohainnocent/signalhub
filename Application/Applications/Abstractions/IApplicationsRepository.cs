using Domain.Common.Responses;
using Domain.Applications.Requests;
using Domain.Applications;

namespace Application.Applications.Abstractions
{
    public interface IApplicationsRepository
    {
        Task<App> CreateApplicationAsync(AppCreationRequest request);
        Task<PagedResultResponse<App>> GetApplicationsAsync(
            int pageNumber, 
            int pageSize, 
            string? search = null, 
            int? tenantId = null,
            int? ownerUserId = null,
            bool? isActive = null);
        Task<App?> GetApplicationByIdAsync(int applicationId);
        Task<App?> GetApplicationByApiKeyAsync(string apiKey);
        Task<App> UpdateApplicationAsync(AppUpdateRequest request);
        Task<bool> DeactivateApplicationAsync(int applicationId);
        Task<bool> ActivateApplicationAsync(int applicationId);
        Task<bool> DeleteApplicationAsync(int applicationId);
        Task<int> CountApplicationsAsync();
        Task<bool> RegenerateApiKeyAsync(int applicationId);
    }
}