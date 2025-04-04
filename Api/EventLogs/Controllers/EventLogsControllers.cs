
using Domain.EventLogs.Requests;
using Application.EventLogs.Abstractions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Claims;
using DataAccess.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.EventLogs.Controllers
{
    public static class EventLogsControllers
    {
        public static async Task<IResult> LogEventAsync(
            IEventLogsRepository repo,
            [FromBody] EventLogCreationRequest request,
            HttpContext httpContext)
        {
            try
            {
                // Ensure the user is authenticated
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to log event for user ID: {UserId}", userId);

                // Log the event
                var createdEventLog = await repo.LogEventAsync(request);

                Log.Information("Event logged successfully with ID: {EventId}", createdEventLog.EventId);
                return Results.Created($"/eventlogs/{createdEventLog.EventId}", createdEventLog);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while logging the event.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetEventsAsync(
            IEventLogsRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            string? entityType = null,
            string? entityId = null,
            string? eventType = null,
            int? createdByUserId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? searchQuery = null)
        {
            try
            {
                Log.Information("Attempting to retrieve events with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                // Retrieve paginated events
                var pagedResult = await repo.GetEventsAsync(pageNumber, pageSize, entityType, entityId, eventType, createdByUserId, fromDate, toDate, searchQuery);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No events found." });
                }

                Log.Information("Successfully retrieved {EventCount} events out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving events.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetEventByIdAsync(
            IEventLogsRepository repo,
            int eventId)
        {
            try
            {
                Log.Information("Attempting to retrieve event with ID: {EventId}", eventId);

                // Retrieve the event by ID
                var eventLog = await repo.GetEventByIdAsync(eventId);

                if (eventLog == null)
                {
                    Log.Warning("Event with ID: {EventId} not found.", eventId);
                    return Results.NotFound(new { message = "Event not found." });
                }

                Log.Information("Successfully retrieved event with ID: {EventId}", eventId);
                return Results.Ok(eventLog);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the event.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetEntityActivityAsync(
            IEventLogsRepository repo,
            string entityType,
            string entityId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                Log.Information("Attempting to retrieve activity for entity: {EntityType}, {EntityId}.", entityType, entityId);

                // Retrieve paginated events for a specific entity
                var pagedResult = await repo.GetEntityActivityAsync(entityType, entityId, pageNumber, pageSize);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No events found for the entity." });
                }

                Log.Information("Successfully retrieved {EventCount} events for the entity.", pagedResult.Items.Count());
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the entity activity.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetEntityTimelineAsync(
            IEventLogsRepository repo,
            string entityType,
            string entityId,
            int limit = 100)
        {
            try
            {
                Log.Information("Attempting to retrieve entity timeline for entity: {EntityType}, {EntityId}.", entityType, entityId);

                // Retrieve the entity timeline
                var timeline = await repo.GetEntityTimelineAsync(entityType, entityId, limit);

                if (timeline == null || !timeline.Any())
                {
                    return Results.NotFound(new { message = "No timeline events found for the entity." });
                }

                Log.Information("Successfully retrieved {TimelineCount} timeline events for the entity.", timeline.Count);
                return Results.Ok(timeline);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the entity timeline.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetFrequentEventsAsync(
            IEventLogsRepository repo,
            TimeSpan period,
            string? entityTypeFilter = null)
        {
            try
            {
                Log.Information("Attempting to retrieve frequent events in the last {Period}.", period);

                // Retrieve frequent events
                var frequentEvents = await repo.GetFrequentEventsAsync(period, entityTypeFilter);

                Log.Information("Successfully retrieved {EventCount} frequent events.", frequentEvents.Count);
                return Results.Ok(frequentEvents);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving frequent events.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> ArchiveEventsAsync(
            IEventLogsRepository repo,
            DateTime cutoffDate)
        {
            try
            {
                Log.Information("Attempting to archive events before {CutoffDate}.", cutoffDate);

                // Archive events
                var archivedCount = await repo.ArchiveEventsAsync(cutoffDate);

                Log.Information("Successfully archived {ArchivedCount} events.", archivedCount);
                return Results.Ok(new { message = $"{archivedCount} events archived successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while archiving events.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> PurgeEventsAsync(
            IEventLogsRepository repo,
            DateTime cutoffDate)
        {
            try
            {
                Log.Information("Attempting to purge events before {CutoffDate}.", cutoffDate);

                // Purge events
                var purgedCount = await repo.PurgeEventsAsync(cutoffDate);

                Log.Information("Successfully purged {PurgedCount} events.", purgedCount);
                return Results.Ok(new { message = $"{purgedCount} events purged successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while purging events.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
