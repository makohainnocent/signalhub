using Api.Common.Abstractions;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Domain.PermitManagement.Requests;
using Microsoft.AspNetCore.Mvc;
using Api.PermitManagement.PermitManagementControllers;
using DataAccess.PermitManagement.Repositories;

namespace Api.PermitManagement.EndpointDefinitions
{
    public class PermitManagementEndpoints : IEndpointDefinition
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

            var permitApplications = versionedGroup.MapGroup("/permit-applications");
            var permits = versionedGroup.MapGroup("/permits");

            // Permit Application Endpoints
            permitApplications.MapPost("/", async (IPermitRepository repo, [FromBody] PermitApplicationCreationRequest request, HttpContext httpContext) =>
            {
                return await PermitManagementController.CreatePermitApplication(repo, request, httpContext);
            })
            .RequireAuthorization()
            .WithTags("Permit Applications");

            permitApplications.MapGet("/", async (
                IPermitRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] int? userId = null,
                [FromQuery] int? permitId = null,
                [FromQuery] string? type = null,
                [FromQuery] string? agent = "no") =>
            {
                return await PermitManagementController.GetAllPermitApplicationsAsync(repo, pageNumber, pageSize, search, userId, permitId, type,agent);
            })
            .WithTags("Permit Applications");

            permitApplications.MapGet("/{applicationId}", async (IPermitRepository repo, int applicationId) =>
            {
                return await PermitManagementController.GetPermitApplicationByIdAsync(repo, applicationId);
            })
            .RequireAuthorization()
            .WithTags("Permit Applications");

            permitApplications.MapPut("/", async (IPermitRepository repo, [FromBody] PermitApplicationUpdateRequest request) =>
            {
                return await PermitManagementController.UpdatePermitApplication(repo, request);
            })
            .RequireAuthorization()
            .WithTags("Permit Applications");

            permitApplications.MapDelete("/{applicationId}", async (IPermitRepository repo, int applicationId) =>
            {
                return await PermitManagementController.DeletePermitApplication(repo, applicationId);
            })
            .RequireAuthorization()
            .WithTags("Permit Applications");

            permitApplications.MapGet("/count", async (IPermitRepository repo, [FromQuery] int? applicantId = null, [FromQuery] int? applicantType = null) =>
            {
                return await PermitManagementController.CountPermitApplicationsAsync(repo, applicantId, applicantType);
            })
            .WithTags("Permit Applications");

            permitApplications.MapGet("/count-pending", async (IPermitRepository repo) =>
            {
                return await PermitManagementController.CountPendingPermitApplicationsAsync(repo);
            })
            .WithTags("Permit Applications");

            // Permit Endpoints
            permits.MapPost("/", async (IPermitRepository repo, [FromBody] PermitCreationRequest request) =>
            {
                return await PermitManagementController.CreatePermitAsync(repo, request);
            })
            .RequireAuthorization()
            .WithTags("Permits");

            permits.MapGet("/{permitId}", async (IPermitRepository repo, int permitId) =>
            {
                return await PermitManagementController.GetPermitByIdAsync(repo, permitId);
            })
            .WithTags("Permits");

            permits.MapPut("/", async (IPermitRepository repo, [FromBody] PermitUpdateRequest request) =>
            {
                return await PermitManagementController.UpdatePermitAsync(repo, request);
            })
            .RequireAuthorization()
            .WithTags("Permits");

            permits.MapDelete("/{permitId}", async (IPermitRepository repo, int permitId) =>
            {
                return await PermitManagementController.DeletePermitAsync(repo, permitId);
            })
            .RequireAuthorization()
            .WithTags("Permits");

            permits.MapGet("/count", async (IPermitRepository repo, [FromQuery] string? permitName = null) =>
            {
                return await PermitManagementController.CountPermitsAsync(repo, permitName);
            })
            .WithTags("Permits");

            permits.MapGet("/", async (
    IPermitRepository repo,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? search = null) =>
            {
                return await PermitManagementController.GetAllPermitsAsync(repo, pageNumber, pageSize, search);
            })
.WithTags("Permits");
        }
    }
}