using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.AnimalManagement.Abstractions;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Domain.AnimalManagement.Requests;
using Microsoft.AspNetCore.Mvc;
using Domain.Core.Models;
using Api.AnimalManagement.Controllers;

namespace Api.AnimalManagement.EndPointDefinations
{
    public class AnimalManagementEndpoints : IEndpointDefinition
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

            var animals = versionedGroup.MapGroup("/animals");

            // Animal-related endpoints
            animals.MapPost("/", async (IAnimalManagementRepository repo, [FromBody] AnimalCreationRequest request, HttpContext httpContext) =>
            {
                return await AnimalManagementControllers.CreateAnimalAsync(repo, request, httpContext);
            })
            .AddEndpointFilter<ValidationFilter<AnimalCreationRequest>>()
            .WithTags("Animal Management");

            animals.MapGet("/premises/{premisesId}", async (IAnimalManagementRepository repo, int premisesId, int pageNumber = 1, int pageSize = 10, string? search = null) =>
            {
                return await AnimalManagementControllers.GetAnimalsByPremisesAsync(repo, premisesId, pageNumber, pageSize, search);
            })
            .WithTags("Animal Management");

            animals.MapGet("/{animalId}", async (IAnimalManagementRepository repo, int animalId) =>
            {
                return await AnimalManagementControllers.GetAnimalByIdAsync(repo, animalId);
            })
            .WithTags("Animal Management");

            animals.MapPut("/{animalId}", async (IAnimalManagementRepository repo, int animalId, [FromBody] AnimalUpdateRequest request, HttpContext httpContext) =>
            {
                request.AnimalId = animalId;
                return await AnimalManagementControllers.UpdateAnimalAsync(repo, request, httpContext);
            })
            .RequireAuthorization()
            .WithTags("Animal Management");

            animals.MapDelete("/{animalId}", async (IAnimalManagementRepository repo, int animalId, HttpContext httpContext) =>
            {
                return await AnimalManagementControllers.DeleteAnimalAsync(repo, animalId, httpContext);
            })
            .RequireAuthorization()
            .WithTags("Animal Management");

            animals.MapGet("/count", async (IAnimalManagementRepository repo, int? ownerId = null, int? premisesId = null) =>
            {
                return await AnimalManagementControllers.CountAnimalsAsync(repo, ownerId, premisesId);
            })
            .WithTags("Animal Management");

            // HealthRecord-related endpoints
            animals.MapPost("/healthrecords", async (IAnimalManagementRepository repo, [FromBody] HealthRecordCreationRequest request, HttpContext httpContext) =>
            {
                return await AnimalManagementControllers.CreateHealthRecordAsync(repo, request, httpContext);
            })
            .WithTags("Health Record Management");

            animals.MapGet("/healthrecords/animal/{animalId}", async (IAnimalManagementRepository repo, int animalId, int pageNumber = 1, int pageSize = 10, string? search = null) =>
            {
                return await AnimalManagementControllers.GetHealthRecordsByAnimalIdAsync(repo, animalId, pageNumber, pageSize, search);
            })
            .WithTags("Health Record Management");

            animals.MapGet("/healthrecords/{healthRecordId}", async (IAnimalManagementRepository repo, int healthRecordId) =>
            {
                return await AnimalManagementControllers.GetHealthRecordByIdAsync(repo, healthRecordId);
            })
            .WithTags("Health Record Management");

            animals.MapPut("/healthrecords/{healthRecordId}", async (IAnimalManagementRepository repo, int healthRecordId, [FromBody] UpdateHealthRecordRequest request) =>
            {
                return await AnimalManagementControllers.UpdateHealthRecordAsync(repo, healthRecordId, request);
            })
            .WithTags("Health Record Management");

            animals.MapDelete("/healthrecords/{healthRecordId}", async (IAnimalManagementRepository repo, int healthRecordId) =>
            {
                return await AnimalManagementControllers.DeleteHealthRecordAsync(repo, healthRecordId);
            })
            .WithTags("Health Record Management");

            animals.MapGet("/healthrecords/count", async (IAnimalManagementRepository repo, int? userId = null, int? animalId = null, int? premisesId = null) =>
            {
                return await AnimalManagementControllers.CountHealthRecordsAsync(repo, userId, animalId, premisesId);
            })
            .WithTags("Health Record Management");

            animals.MapGet("/healthrecords/all", async (IAnimalManagementRepository repo, int pageNumber = 1, int pageSize = 10, string? search = null, int? userId = null, int? animalId = null, int? premisesId = null) =>
            {
                return await AnimalManagementControllers.GetAllHealthRecordsAsync(repo, pageNumber, pageSize, search, userId, animalId, premisesId);
            })
            .WithTags("Health Record Management");
        }
    }
}