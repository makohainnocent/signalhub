using Api.Common.Abstractions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Domain.Tenants.Requests;
using Microsoft.AspNetCore.Mvc;
using Application.Tenants.Abtstractions;
using Api.Tenants.Controllers;

namespace Api.Tenants.EndPointDefinitions
{
    public class TenantsEndpoints : IEndpointDefinition
    {
        public void RegisterEndpoints(WebApplication app)
        {
            ApiVersionSet apiVersionSet = app.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1))
                .ReportApiVersions()
                .Build();

            RouteGroupBuilder versionedGroup = app
                .MapGroup("/api/v{apiVersion:apiVersion}")
                .WithApiVersionSet(apiVersionSet);

            var tenants = versionedGroup.MapGroup("/tenants")
                .WithTags("Tenants Management");

            // Create a new tenant
            tenants.MapPost("/", async (
                ITenantsRepository repo,
                [FromBody] TenantCreationRequest request) =>
            {
                return await TenantsController.CreateTenantAsync(repo, request);
            });

            // Get tenants with pagination and filters
            tenants.MapGet("/", async (
                ITenantsRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] bool? isActive = null,
                [FromQuery] int? ownerUserId = null) =>
            {
                return await TenantsController.GetTenantsAsync(
                    repo, pageNumber, pageSize, search, isActive, ownerUserId);
            });

            // Get tenant by ID
            tenants.MapGet("/{tenantId:int}", async (
                ITenantsRepository repo,
                int tenantId) =>
            {
                return await TenantsController.GetTenantByIdAsync(repo, tenantId);
            });

            // Get tenant by slug
            tenants.MapGet("/by-slug/{slug}", async (
                ITenantsRepository repo,
                string slug) =>
            {
                return await TenantsController.GetTenantBySlugAsync(repo, slug);
            });

            // Update tenant
            tenants.MapPut("/", async (
                ITenantsRepository repo,
                [FromBody] TenantUpdateRequest request) =>
            {
                return await TenantsController.UpdateTenantAsync(repo, request);
            });

            // Deactivate tenant
            tenants.MapPut("/{tenantId:int}/deactivate", async (
                ITenantsRepository repo,
                int tenantId) =>
            {
                return await TenantsController.DeactivateTenantAsync(repo, tenantId);
            });

            // Activate tenant
            tenants.MapPut("/{tenantId:int}/activate", async (
                ITenantsRepository repo,
                int tenantId) =>
            {
                return await TenantsController.ActivateTenantAsync(repo, tenantId);
            });

            // Delete tenant
            tenants.MapDelete("/{tenantId:int}", async (
                ITenantsRepository repo,
                int tenantId) =>
            {
                return await TenantsController.DeleteTenantAsync(repo, tenantId);
            });

            // Count tenants
            tenants.MapGet("/count", async (
                ITenantsRepository repo) =>
            {
                return await TenantsController.CountTenantsAsync(repo);
            });
        }
    }
}