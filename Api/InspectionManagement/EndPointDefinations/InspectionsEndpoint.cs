using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.InspectionManagement.Abstractions;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Domain.Core.Models;
using Domain.InspectionManagement.Requests;
using Microsoft.AspNetCore.Mvc;
using Api.InspectionManagement.Controllers;



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

            inspections.MapGet("/", async (
                IInspectionRepository repo, 
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null, 
                [FromQuery] int? userId = 0, 
                [FromQuery] int? animalId = 0,
                [FromQuery] int? premiseId = 0,
                [FromQuery] int? tagId = 0,
                [FromQuery] int? productId = 0,
                [FromQuery] int? transportId = 0
                ) =>
            {
                return await InspectionsControllers.GetAllInspectionsAsync
                (
                    repo, 
                    pageNumber, 
                    pageSize, 
                    search,
                    userId,
                    animalId,
                    premiseId,
                    tagId,
                    productId,
                    transportId
                )
                    ;
            })
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

            inspections.MapGet("/inspections/count", async (
                IInspectionRepository repo,
                 int? userId = 0,
                int? animalId = 0,
                int? premiseId = 0,
                int? tagId = 0,
                int? productId = 0,
                int? transportId = 0
               ) =>
            {
                return await InspectionsControllers.CountInspections(
                    repo,
                 userId,
                    animalId,
                    premiseId,
                    tagId,
                    productId,
                    transportId
                    );
            })
         // .RequireAuthorization()  // Uncomment if authorization is required
         .WithTags("Inspections");

            inspections.MapGet("/inspections/compliant-inspections-this-Week", async (IInspectionRepository repo) =>
            {
                return await InspectionsControllers.GetCompliantInspectionsThisWeekAsync(repo);
            })
            .WithTags("Inspections");

            inspections.MapGet("/inspections/none-compliant-inspections-this-Week", async (IInspectionRepository repo) =>
            {
                return await InspectionsControllers.GetNonCompliantInspectionsThisWeekAsync(repo);
            })
            .WithTags("Inspections");

            inspections.MapGet("/inspections/total-inspections-this-Week", async (IInspectionRepository repo) =>
            {
                return await InspectionsControllers.GetInspectionsThisWeek(repo);
            })
            .WithTags("Inspections");
        }
    }
}
