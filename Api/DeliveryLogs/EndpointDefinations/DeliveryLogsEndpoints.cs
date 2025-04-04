using Api.Common.Abstractions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Application.DeliveryLogs.Abstractions;
using Domain.DeliveryLogs.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Api.DeliveryLogs.Controllers;

namespace Api.DeliveryLogs.EndPointDefinitions
{
    public class DeliveryLogsEndpoints : IEndpointDefinition
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

            var deliveryLogs = versionedGroup.MapGroup("/deliverylogs")
                .WithTags("Delivery Logs Management");

            // Create a new delivery log
            deliveryLogs.MapPost("/", async (IDeliveryLogsRepository repo, [FromBody] DeliveryLogCreationRequest request, HttpContext httpContext) =>
            {
                return await DeliveryLogsControllers.CreateLogAsync(repo, request, httpContext);
            })
            .RequireAuthorization();

            // Get all logs for a specific delivery (paginated)
            deliveryLogs.MapGet("/by-delivery/{deliveryId:int}", async (
                IDeliveryLogsRepository repo,
                int deliveryId,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? eventType = null,
                [FromQuery] DateTime? fromDate = null,
                [FromQuery] DateTime? toDate = null) =>
            {
                return await DeliveryLogsControllers.GetLogsByDeliveryAsync(repo, deliveryId, pageNumber, pageSize, eventType, fromDate, toDate);
            });

            // Get a specific delivery log by ID
            deliveryLogs.MapGet("/{logId:int}", async (IDeliveryLogsRepository repo, int logId) =>
            {
                return await DeliveryLogsControllers.GetLogByIdAsync(repo, logId);
            });

            // Get event type distribution
            deliveryLogs.MapGet("/event-types", async (IDeliveryLogsRepository repo, [FromQuery] int? deliveryId = null) =>
            {
                return await DeliveryLogsControllers.GetEventTypeDistributionAsync(repo, deliveryId);
            });

        }
    }
}
