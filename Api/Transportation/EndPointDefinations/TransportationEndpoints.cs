// Api/Transportation/EndPointDefinations/TransportationEndpoints.cs
using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.Transportations.Abstractions;
using Domain.Transportation.Requests;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Api.Transportation.Controllers;


namespace Api.Transportation.EndPointDefinations
{
    public class TransportationEndpoints : IEndpointDefinition
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

            var transportations = versionedGroup.MapGroup("/transportations")
                .WithTags("Transportation Management");

            // Create a new transportation
            transportations.MapPost("/", async (ITransportationRepository repo, [FromBody] TransportationCreationRequest request, HttpContext httpContext) =>
            {
                return await TransportationControllers.CreateTransportationAsync(repo, request, httpContext);
            })
            .RequireAuthorization();

            // Get all transportations (paginated)
            transportations.MapGet("/", async (ITransportationRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] int? userId = null, [FromQuery] int? premiseId = null, [FromQuery] string? agent = "no", [FromQuery] string? vet = "no") =>
            {
                return await TransportationControllers.GetTransportationsAsync(repo, pageNumber, pageSize, search,userId,premiseId,agent,vet);
            });

            // Get a transportation by ID
            transportations.MapGet("/{transportId:int}", async (ITransportationRepository repo, int transportId) =>
            {
                return await TransportationControllers.GetTransportationByIdAsync(repo, transportId);
            });

            // Update a transportation
            transportations.MapPut("/", async (ITransportationRepository repo, [FromBody] TransportationUpdateRequest request) =>
            {
                return await TransportationControllers.UpdateTransportationAsync(repo, request);
            })
            .RequireAuthorization();

            // Delete a transportation
            transportations.MapDelete("/{transportId:int}", async (ITransportationRepository repo, int transportId) =>
            {
                return await TransportationControllers.DeleteTransportationAsync(repo, transportId);
            })
            .RequireAuthorization();

            // Count all transportations
            transportations.MapGet("/count", async (ITransportationRepository repo) =>
            {
                return await TransportationControllers.CountTransportationsAsync(repo);
            });
        }
    }
}