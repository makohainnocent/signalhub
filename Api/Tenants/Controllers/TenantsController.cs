using Application.Tenants.Abtstractions;
using Domain.Common.Responses;
using Domain.Tenants;
using Domain.Tenants.Requests;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Api.Tenants.Controllers
{
    
    [ApiController]
    public static class TenantsController
    {
        // Create a new tenant
        public static async Task<IResult> CreateTenantAsync(
            ITenantsRepository repo,
            [FromBody] TenantCreationRequest request)
        {
            try
            {
                Log.Information("Creating new tenant: {TenantName}", request.Name);
                var tenant = await repo.CreateTenantAsync(request);
                return Results.Created($"/api/Tenants/{tenant.TenantId}", tenant);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating tenant.");
                return Results.Problem(ex.Message);
            }
        }

        // Get tenants with pagination and filters
        public static async Task<IResult> GetTenantsAsync(
            ITenantsRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            bool? isActive = null,
            int? ownerUserId = null)
        {
            try
            {
                Log.Information("Fetching tenants (Page {PageNumber}, Size {PageSize})", pageNumber, pageSize);
                var tenants = await repo.GetTenantsAsync(pageNumber, pageSize, search, isActive, ownerUserId);
                return Results.Ok(tenants);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching tenants.");
                return Results.Problem(ex.Message);
            }
        }

        // Get tenant by ID
        public static async Task<IResult> GetTenantByIdAsync(
            ITenantsRepository repo,
            int tenantId)
        {
            try
            {
                Log.Information("Fetching tenant with ID {TenantId}", tenantId);
                var tenant = await repo.GetTenantByIdAsync(tenantId);
                return tenant != null ? Results.Ok(tenant) : Results.NotFound(new { message = "Tenant not found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching tenant.");
                return Results.Problem(ex.Message);
            }
        }

        // Get tenant by slug
        public static async Task<IResult> GetTenantBySlugAsync(
            ITenantsRepository repo,
            string slug)
        {
            try
            {
                Log.Information("Fetching tenant by slug: {Slug}", slug);
                var tenant = await repo.GetTenantBySlugAsync(slug);
                return tenant != null ? Results.Ok(tenant) : Results.NotFound(new { message = "Tenant not found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching tenant by slug.");
                return Results.Problem(ex.Message);
            }
        }

        // Update tenant
        public static async Task<IResult> UpdateTenantAsync(
            ITenantsRepository repo,
            [FromBody] TenantUpdateRequest request)
        {
            try
            {
                Log.Information("Updating tenant with ID {TenantId}", request.TenantId);
                var updatedTenant = await repo.UpdateTenantAsync(request);
                return Results.Ok(updatedTenant);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating tenant.");
                return Results.Problem(ex.Message);
            }
        }

        // Deactivate tenant
        public static async Task<IResult> DeactivateTenantAsync(
            ITenantsRepository repo,
            int tenantId)
        {
            try
            {
                Log.Information("Deactivating tenant with ID {TenantId}", tenantId);
                var success = await repo.DeactivateTenantAsync(tenantId);
                return success ? Results.Ok(new { message = "Tenant deactivated successfully." }) : Results.BadRequest(new { message = "Failed to deactivate tenant." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deactivating tenant.");
                return Results.Problem(ex.Message);
            }
        }

        // Activate tenant
        public static async Task<IResult> ActivateTenantAsync(
            ITenantsRepository repo,
            int tenantId)
        {
            try
            {
                Log.Information("Activating tenant with ID {TenantId}", tenantId);
                var success = await repo.ActivateTenantAsync(tenantId);
                return success ? Results.Ok(new { message = "Tenant activated successfully." }) : Results.BadRequest(new { message = "Failed to activate tenant." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error activating tenant.");
                return Results.Problem(ex.Message);
            }
        }

        // Delete tenant
        public static async Task<IResult> DeleteTenantAsync(
            ITenantsRepository repo,
            int tenantId)
        {
            try
            {
                Log.Information("Deleting tenant with ID {TenantId}", tenantId);
                var success = await repo.DeleteTenantAsync(tenantId);
                return success ? Results.Ok(new { message = "Tenant deleted successfully." }) : Results.BadRequest(new { message = "Failed to delete tenant." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting tenant.");
                return Results.Problem(ex.Message);
            }
        }

        // Count tenants
        public static async Task<IResult> CountTenantsAsync(ITenantsRepository repo)
        {
            try
            {
                Log.Information("Counting total tenants.");
                var count = await repo.CountTenantsAsync();
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error counting tenants.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
