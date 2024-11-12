using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.FarmManagement.Abstractions;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Domain.FarmManagement.Requests;
using Application.LivestockManagement.Abstractions;
using Domain.LivestockManagement.Requests;
using Api.LivestockManagement.Controllers;
using Microsoft.AspNetCore.Mvc;
using Domain.Core.Models;

namespace Api.LivestockManagement.EndPointDefinations
{
    public class LivestockManagementEndpoints:IEndpointDefinition
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

            var livestock = versionedGroup.MapGroup("/livestock");

            livestock.MapPost("/livestock", async (ILivestockManagementRepository repo, [FromBody] LivestockCreationRequest request, HttpContext httpContext) =>
            {
                return await LivestockManagementControllers.CreateLivestock(repo, request, httpContext);
            })
            //.RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<LivestockCreationRequest>>()
            .WithTags("Livestock Management");

            livestock.MapGet("/livestock-all/{farmId}", async (ILivestockManagementRepository repo, int farmId, int pageNumber=1, int pageSize=10, string? search = null) =>
            {
                return await LivestockManagementControllers.GetLivestockByFarm(repo, farmId, pageNumber, pageSize, search);
            })
            //.RequireAuthorization()
            .WithTags("Livestock Management");

            livestock.MapGet("/livestock/{livestockId}", async (ILivestockManagementRepository repo, int livestockId) =>
            {
                return await LivestockManagementControllers.GetLivestockById(repo, livestockId);
            })
            //.RequireAuthorization()
            .WithTags("Livestock Management");

            livestock.MapGet("/livestock/count", async (ILivestockManagementRepository repo, int? userId = null, int? farmId = null) =>
            {
                return await LivestockManagementControllers.CountLivestock(repo, userId, farmId);
            })
            // .RequireAuthorization()  // Uncomment if authorization is required
           .WithTags("Livestock Management");


            livestock.MapPut("/livestock/{livestockId}", async (ILivestockManagementRepository repo, int livestockId, [FromBody] LivestockUpdateRequest request, HttpContext httpContext) =>
            {
                request.LivestockId = livestockId;
                return await LivestockManagementControllers.UpdateLivestock(repo, request, httpContext);
            })
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<LivestockUpdateRequest>>()
            .WithTags("Livestock Management");

            livestock.MapDelete("/livestock/{livestockId}", async (ILivestockManagementRepository repo, int livestockId, HttpContext httpContext) =>
            {
                return await LivestockManagementControllers.DeleteLivestock(repo, livestockId, httpContext);
            })
            .RequireAuthorization()
            .WithTags("Livestock Management");

            livestock.MapPost("/healthrecords", async (HealthRecordCreationRequest request, ILivestockManagementRepository repo, HttpContext httpContext) =>
            {
                return await LivestockManagementControllers.CreateHealthRecord(request, repo, httpContext);
            })
            .WithTags("Health Record Management");

            livestock.MapGet("/healthrecords/{livestockId}", async (ILivestockManagementRepository repo, int livestockId, int pageNumber = 1, int pageSize = 10, string? search = null) =>
            {
                return await LivestockManagementControllers.GetHealthRecordsByLivestock(repo, livestockId, pageNumber, pageSize, search);
            })
            // .RequireAuthorization()
            .WithTags("Health Record Management");

            livestock.MapGet("/healthrecord/{healthRecordId}", async (ILivestockManagementRepository repo, int healthRecordId) =>
            {
                return await LivestockManagementControllers.GetHealthRecordById(repo, healthRecordId);
            })
            // .RequireAuthorization()
            .WithTags("Health Record Management");

            livestock.MapPut("/healthrecord/{healthRecordId}", async (ILivestockManagementRepository repo, int healthRecordId, UpdateHealthRecordRequest updatedHealthRecord) =>
            {
                return await LivestockManagementControllers.UpdateHealthRecord(repo, healthRecordId, updatedHealthRecord);
            })
            // .RequireAuthorization()
            .WithTags("Health Record Management");

            livestock.MapDelete("/healthrecord/{healthRecordId}", async (ILivestockManagementRepository repo, int healthRecordId) =>
            {
                return await LivestockManagementControllers.DeleteHealthRecord(repo, healthRecordId);
            })
            // .RequireAuthorization()
            .WithTags("Health Record Management");

            livestock.MapGet("/healthrecords/count", async (ILivestockManagementRepository repo, int? userId = null, int ? livestockId = null, int ? farmId = null) =>
            {
                return await LivestockManagementControllers.CountHealthRecords(repo, userId,livestockId, farmId);
            })
          // .RequireAuthorization()  // Uncomment if authorization is required
          .WithTags("Health Record Management");

            livestock.MapPost("/directive", async (ILivestockManagementRepository repo, CreateDirectiveRequest newDirective) =>
            {
                return await LivestockManagementControllers.CreateDirective(repo, newDirective);
            })
            // .RequireAuthorization()
            .WithTags("Directive Management");

            livestock.MapGet("/directives/{livestockId}", async (ILivestockManagementRepository repo, int livestockId, int pageNumber = 1, int pageSize = 10, string? search = null) =>
            {
                return await LivestockManagementControllers.GetDirectivesByLivestock(repo, livestockId, pageNumber, pageSize, search);
            })
            // .RequireAuthorization()
            .WithTags("Directive Management");

            livestock.MapGet("/directive/{directiveId}", async (ILivestockManagementRepository repo, int directiveId) =>
            {
                return await LivestockManagementControllers.GetDirectiveById(repo, directiveId);
            })
            // .RequireAuthorization()
            .WithTags("Directive Management");

            livestock.MapPut("/directive/{directiveId}", async (ILivestockManagementRepository repo, int directiveId, UpdateDirectiveRequest updateDirective) =>
            {
                return await LivestockManagementControllers.UpdateDirective(repo, directiveId, updateDirective);
            })
            // .RequireAuthorization()
            .WithTags("Directive Management");

            livestock.MapDelete("/directive/{directiveId}", async (ILivestockManagementRepository repo, int directiveId) =>
            {
                return await LivestockManagementControllers.DeleteDirective(repo, directiveId);
            })
            // .RequireAuthorization()
            .WithTags("Directive Management");



        }
    }
}
