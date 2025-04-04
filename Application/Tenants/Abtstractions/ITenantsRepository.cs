using Domain.Common.Responses;
using Domain.Tenants;
using Domain.Tenants.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tenants.Abtstractions
{
    public interface ITenantsRepository
    {
        Task<Tenant> CreateTenantAsync(TenantCreationRequest request);
        Task<PagedResultResponse<Tenant>> GetTenantsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            bool? isActive = null,
            int? ownerUserId = null);
        Task<Tenant?> GetTenantByIdAsync(int tenantId);
        Task<Tenant?> GetTenantBySlugAsync(string slug);
        Task<Tenant> UpdateTenantAsync(TenantUpdateRequest request);
        Task<bool> DeactivateTenantAsync(int tenantId);
        Task<bool> ActivateTenantAsync(int tenantId);
        Task<bool> DeleteTenantAsync(int tenantId);
        Task<int> CountTenantsAsync();
    }
}

