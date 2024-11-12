using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.InspectionManagement.Abstractions;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Domain.Core.Models;
using Domain.InspectionManagement.Requests;
using Microsoft.AspNetCore.Mvc;
using Api.InspectionManagement.Controllers;
using Api.LivestockManagement.Controllers;
using Application.LivestockManagement.Abstractions;

namespace Api.InspectionManagement.EndPointDefinations
{
    public class InspectionsEndpoint : IEndpointDefinition
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

            var inspections = versionedGroup.MapGroup("/inspections");

            inspections.MapPost("/", async (IInspectionRepository repo, [FromBody] CreateInspectionRequest request, HttpContext httpContext) =>
            {
                return await InspectionsControllers.CreateInspectionAsync(repo, request, httpContext);
            })
            .RequireAuthorization()
            .WithTags("Inspections");

            inspections.MapGet("/", async (IInspectionRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] int? userId = 0) =>
            {
                return await InspectionsControllers.GetAllInspectionsAsync(repo, pageNumber, pageSize, search,userId);
            })
            .RequireAuthorization()
            .WithTags("Inspections");

            inspections.MapGet("/{inspectionId}", async (IInspectionRepository repo, int inspectionId) =>
            {
                return await InspectionsControllers.GetInspectionByIdAsync(repo, inspectionId);
            })
            .RequireAuthorization()
            .WithTags("Inspections");

            inspections.MapPut("/{inspectionId}", async (IInspectionRepository repo, int inspectionId, [FromBody] Inspection inspection) =>
            {
                return await InspectionsControllers.UpdateInspectionAsync(repo, inspectionId, inspection);
            })
            .RequireAuthorization()
            .WithTags("Inspections");

            inspections.MapDelete("/{inspectionId}", async (IInspectionRepository repo, int inspectionId) =>
            {
                return await InspectionsControllers.DeleteInspectionAsync(repo, inspectionId);
            })
            .RequireAuthorization()
            .WithTags("Inspections");

            inspections.MapGet("/inspections/count", async (IInspectionRepository repo, int? userId = null, int? livestockId = null, int? farmId = null) =>
            {
                return await InspectionsControllers.CountInspections(repo, userId, livestockId, farmId);
            })
         // .RequireAuthorization()  // Uncomment if authorization is required
         .WithTags("Inspections");
        }
    }
}
