using Api.Common.Abstractions;
using Application.PremiseOwners.Abstraction;
using Domain.PremiseOwner.PremiseOwnerCreateRequest;
using Domain.PremiseOwner.Requests;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Api.PremiseOwners.Controllers;
using Asp.Versioning.Builder;
using Asp.Versioning;

namespace Api.PremiseOwner.EndpointDefinitions
{
    public class PremiseOwnerEndpoints : IEndpointDefinition
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

            var premiseOwner = versionedGroup.MapGroup("/premiseowners");

            premiseOwner.MapPost("/", async (IPremiseOwnerRepository repo, [FromBody] PremiseOwnerCreateRequest request, HttpContext httpContext) =>
            {
                return await PremiseOwnerController.CreatePremiseOwnerAsync(repo, request, httpContext);
            })
            .RequireAuthorization()
            .WithTags("Premise Owner Management");

            premiseOwner.MapGet("/all", async (IPremiseOwnerRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, int? registerdBy = 0) =>
            {
                return await PremiseOwnerController.GetPremiseOwnersAsync(repo, pageNumber, pageSize, search,registerdBy);
            })
            .WithTags("Premise Owner Management");

            premiseOwner.MapGet("/{premiseOwnerId}", async (IPremiseOwnerRepository repo, int premiseOwnerId) =>
            {
                return await PremiseOwnerController.GetPremiseOwnerByIdAsync(repo, premiseOwnerId);
            })
            .RequireAuthorization()
            .WithTags("Premise Owner Management");

            premiseOwner.MapPut("/", async (IPremiseOwnerRepository repo, [FromBody] PremiseOwnerUpdateRequest request) =>
            {
                return await PremiseOwnerController.UpdatePremiseOwnerAsync(repo, request);
            })
            .RequireAuthorization()
            .WithTags("Premise Owner Management");

            premiseOwner.MapDelete("/{premiseOwnerId}", async (IPremiseOwnerRepository repo, int premiseOwnerId) =>
            {
                return await PremiseOwnerController.DeletePremiseOwnerAsync(repo, premiseOwnerId);
            })
            .RequireAuthorization()
            .WithTags("Premise Owner Management");
        }
    }
}
