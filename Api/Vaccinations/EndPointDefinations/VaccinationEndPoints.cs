using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.Vaccinations.Abstractions;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Domain.Core.Models.Domain.Core.Models;
using Domain.Vaccinations.Requests;
using Api.Vaccinations.VaccinationsControllers;
using Microsoft.AspNetCore.Mvc;
using Domain.Common.Responses;
using Api.LivestockManagement.Controllers;
using Application.LivestockManagement.Abstractions;
using Domain.Core.Models;

namespace Api.Vaccinations.EndPointDefinations
{
    public class VaccinationsEndpoint : IEndpointDefinition
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

            var vaccinations = versionedGroup.MapGroup("/vaccinations");

            vaccinations.MapPost("/", async (IVaccinationRepository repo, [FromBody] CreateVaccinationRequest request, HttpContext httpContext) =>
            {
                return await VaccinationControllers.CreateVaccinationAsync(repo, request, httpContext);
            })
            .RequireAuthorization()
            .WithTags("Vaccinations");
                                                                    
            vaccinations.MapGet("/", async (IVaccinationRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? farmId = null, [FromQuery] int? userId = null, [FromQuery] string? search = null) =>
            {
                return await VaccinationControllers.GetAllVaccinationsAsync(repo, pageNumber, pageSize,farmId,userId, search);
            })
            .RequireAuthorization()
            .WithTags("Vaccinations");

            vaccinations.MapGet("/{vaccinationId}", async (IVaccinationRepository repo, int vaccinationId) =>
            {
                return await VaccinationControllers.GetVaccinationByIdAsync(repo, vaccinationId);
            })
            .RequireAuthorization()
            .WithTags("Vaccinations");

            vaccinations.MapPut("/{vaccinationId}", async (IVaccinationRepository repo, int vaccinationId, [FromBody] Vaccination vaccination) =>
            {
                return await VaccinationControllers.UpdateVaccinationAsync(repo, vaccinationId, vaccination);
            })
            .RequireAuthorization()
            .WithTags("Vaccinations");

            vaccinations.MapDelete("/{vaccinationId}", async (IVaccinationRepository repo, int vaccinationId) =>
            {
                return await VaccinationControllers.DeleteVaccinationAsync(repo, vaccinationId);
            })
            .RequireAuthorization()
            .WithTags("Vaccinations");

            vaccinations.MapGet("/vaccinations/count", async (IVaccinationRepository repo, int? userId = null, int? farmId = null, int? livestockId = null) =>
            {
                return await VaccinationControllers.CountVaccinations(repo, userId, farmId,livestockId);
            })
         
          .WithTags("Vaccinations");
        }
    }
}
