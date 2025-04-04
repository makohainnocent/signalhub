
using Domain.DeliveryLogs.Requests;
using Application.DeliveryLogs.Abstractions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Claims;
using DataAccess.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.DeliveryLogs.Controllers
{
    public static class DeliveryLogsControllers
    {
        public static async Task<IResult> CreateLogAsync(
            IDeliveryLogsRepository repo,
            [FromBody] DeliveryLogCreationRequest request,
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

                Log.Information("Attempting to create delivery log for user ID: {UserId}", userId);

                // Create the delivery log
                var createdLog = await repo.CreateLogAsync(request);

                Log.Information("Delivery log created successfully with ID: {LogId}", createdLog.LogId);
                return Results.Created($"/deliverylogs/{createdLog.LogId}", createdLog);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the delivery log.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetLogsByDeliveryAsync(
            IDeliveryLogsRepository repo,
            int deliveryId,
            int pageNumber = 1,
            int pageSize = 10,
            string? eventType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                Log.Information("Attempting to retrieve delivery logs for delivery ID: {DeliveryId}, Page {PageNumber}, PageSize {PageSize}.", deliveryId, pageNumber, pageSize);

                // Retrieve paginated logs for a delivery
                var pagedResult = await repo.GetLogsByDeliveryAsync(deliveryId, pageNumber, pageSize, eventType, fromDate, toDate);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No delivery logs found for this delivery." });
                }

                Log.Information("Successfully retrieved {LogCount} delivery logs for delivery ID: {DeliveryId}.", pagedResult.Items.Count(), deliveryId);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving delivery logs.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetLogByIdAsync(
            IDeliveryLogsRepository repo,
            int logId)
        {
            try
            {
                Log.Information("Attempting to retrieve delivery log with ID: {LogId}", logId);

                // Retrieve the delivery log by ID
                var log = await repo.GetLogByIdAsync(logId);

                if (log == null)
                {
                    Log.Warning("Delivery log with ID: {LogId} not found.", logId);
                    return Results.NotFound(new { message = "Delivery log not found." });
                }

                Log.Information("Successfully retrieved delivery log with ID: {LogId}", logId);
                return Results.Ok(log);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the delivery log.");
                return Results.Problem(ex.Message);
            }
        }

        

        

     

        public static async Task<IResult> GetEventTypeDistributionAsync(
            IDeliveryLogsRepository repo,
            int? deliveryId = null)
        {
            try
            {
                Log.Information("Attempting to retrieve event type distribution for delivery ID: {DeliveryId}.", deliveryId);

                // Retrieve event type distribution
                var eventTypeDistribution = await repo.GetEventTypeDistributionAsync(deliveryId);

                Log.Information("Successfully retrieved event type distribution.");
                return Results.Ok(eventTypeDistribution);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving event type distribution.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
