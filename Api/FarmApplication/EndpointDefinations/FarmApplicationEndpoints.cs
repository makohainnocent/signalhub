using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.FarmManagement.Abstractions;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Domain.FarmManagement.Requests;
using Application.Application.Abstractions;
using Domain.FarmApplication.Requests;
using Microsoft.AspNetCore.Mvc;
using Api.FarmApplication.FarmApplicationControllers;
using Api.FarmManagement.Controllers;
using Domain.Core.Models;
using Api.LivestockManagement.Controllers;
using Application.LivestockManagement.Abstractions;

namespace Api.FarmApplication.EndpointDefinations
{
    public class FarmApplicationEndpoints : IEndpointDefinition
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

            var application = versionedGroup.MapGroup("/application");


            application.MapPost("/applications", async (IApplicationRepository repo, [FromBody] ApplicationCreationRequest request, HttpContext httpContext) =>
            {

                return await FarmApplicationController.CreateApplication(repo, request, httpContext);
            })
            .RequireAuthorization()
            .WithTags("Applications");


            application.MapGet("/applications-all", async (
                IApplicationRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] int? userId = null,
                [FromQuery] int? farmId = null,
                [FromQuery] string ? type = null) =>
            {
                return await FarmApplicationController.GetAllFarmApplicationsAsync(repo, pageNumber, pageSize, search, userId, farmId,type);
            })
            .RequireAuthorization()
            .WithTags("Applications");


            application.MapGet("/applications/{applicationId}", async (IApplicationRepository repo, int applicationId) =>
            {
                return await FarmApplicationController.GetFarmApplicationByIdAsync(repo, applicationId);
            })
            .RequireAuthorization()
            .WithTags("Applications");

            application.MapPut("/applications", async (IApplicationRepository repo, [FromBody] ApplicationUpdateRequest request) =>
            {
                return await FarmApplicationController.UpdateApplication(repo, request);
            })
             .RequireAuthorization()
            .WithTags("Applications");

            application.MapDelete("/applications/{applicationId}", async (IApplicationRepository repo, int applicationId) =>
            {
                return await FarmApplicationController.DeleteApplication(repo, applicationId);
            })
            .RequireAuthorization()
            .WithTags("Applications");

            application.MapGet("/applications/count", async (IApplicationRepository repo, int? userId = null, int? farmId = null) =>
            {
                return await FarmApplicationController.CountApplications(repo, userId, farmId);
            })
          // .RequireAuthorization()  // Uncomment if authorization is required
          .WithTags("Applications");

        }
    }
}
