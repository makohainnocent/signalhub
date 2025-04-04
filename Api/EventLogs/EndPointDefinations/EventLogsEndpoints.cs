using Api.Common.Abstractions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Application.EventLogs.Abstractions;
using Domain.EventLogs.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Api.EventLogs.Controllers;

namespace Api.EventLogs.EndPointDefinitions
{
    public class EventLogsEndpoints : IEndpointDefinition
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

            var eventLogs = versionedGroup.MapGroup("/eventlogs")
                .WithTags("Event Logs Management");

            // Log a new event
            eventLogs.MapPost("/", async (IEventLogsRepository repo, [FromBody] EventLogCreationRequest request, HttpContext httpContext) =>
            {
                return await EventLogsControllers.LogEventAsync(repo, request, httpContext);
            })
            .RequireAuthorization();

            // Get all events (paginated)
            eventLogs.MapGet("/", async (
                IEventLogsRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? entityType = null,
                [FromQuery] string? entityId = null,
                [FromQuery] string? eventType = null,
                [FromQuery] int? createdByUserId = null,
                [FromQuery] DateTime? fromDate = null,
                [FromQuery] DateTime? toDate = null,
                [FromQuery] string? searchQuery = null) =>
            {
                return await EventLogsControllers.GetEventsAsync(repo, pageNumber, pageSize, entityType, entityId, eventType, createdByUserId, fromDate, toDate, searchQuery);
            });

            // Get a specific event log by ID
            eventLogs.MapGet("/{eventId:int}", async (IEventLogsRepository repo, int eventId) =>
            {
                return await EventLogsControllers.GetEventByIdAsync(repo, eventId);
            });

            // Get entity activity logs
            eventLogs.MapGet("/entity-activity/{entityType}/{entityId}", async (
                IEventLogsRepository repo,
                string entityType,
                string entityId,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                return await EventLogsControllers.GetEntityActivityAsync(repo, entityType, entityId, pageNumber, pageSize);
            });

            // Get entity timeline
            eventLogs.MapGet("/entity-timeline/{entityType}/{entityId}", async (
                IEventLogsRepository repo,
                string entityType,
                string entityId,
                [FromQuery] int limit = 100) =>
            {
                return await EventLogsControllers.GetEntityTimelineAsync(repo, entityType, entityId, limit);
            });

            // Get frequent events
            eventLogs.MapGet("/frequent-events", async (
                IEventLogsRepository repo,
                [FromQuery] TimeSpan period,
                [FromQuery] string? entityTypeFilter = null) =>
            {
                return await EventLogsControllers.GetFrequentEventsAsync(repo, period, entityTypeFilter);
            });

            // Archive old events
            eventLogs.MapPost("/archive", async (IEventLogsRepository repo, [FromBody] DateTime cutoffDate) =>
            {
                return await EventLogsControllers.ArchiveEventsAsync(repo, cutoffDate);
            });

            // Purge old events
            eventLogs.MapDelete("/purge", async (IEventLogsRepository repo, [FromBody] DateTime cutoffDate) =>
            {
                return await EventLogsControllers.PurgeEventsAsync(repo, cutoffDate);
            });
        }
    }
}
