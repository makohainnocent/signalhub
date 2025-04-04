
using Domain.MessageDeliveries.Requests;
using Application.MessageDeliveries.Abstractions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using DataAccess.Common.Exceptions;

namespace Api.MessageDeliveries.Controllers
{
    public static class MessageDeliveriesController
    {
        public static async Task<IResult> CreateDeliveryAsync(
            IMessageDeliveriesRepository repo,
            [FromBody] MessageDeliveryCreationRequest request,
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

                Log.Information("Attempting to create message delivery for user ID: {UserId}", userId);

                // Create the delivery
                var createdDelivery = await repo.CreateDeliveryAsync(request);

                Log.Information("Message delivery created successfully with ID: {DeliveryId}", createdDelivery.DeliveryId);
                return Results.Created($"/messagedeliveries/{createdDelivery.DeliveryId}", createdDelivery);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the message delivery.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetDeliveriesAsync(
            IMessageDeliveriesRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            Guid? requestId = null,
            int? recipientId = null,
            int? providerId = null,
            string? channelType = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool? onlyFailed = false)
        {
            try
            {
                Log.Information("Attempting to retrieve deliveries with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                // Retrieve paginated deliveries
                var pagedResult = await repo.GetDeliveriesAsync(pageNumber, pageSize, requestId, recipientId, providerId, channelType, status, fromDate, toDate, onlyFailed);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No deliveries found." });
                }

                Log.Information("Successfully retrieved {DeliveryCount} deliveries out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving deliveries.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetDeliveryByIdAsync(
            IMessageDeliveriesRepository repo,
            int deliveryId)
        {
            try
            {
                Log.Information("Attempting to retrieve message delivery with ID: {DeliveryId}", deliveryId);

                // Retrieve the delivery by ID
                var delivery = await repo.GetDeliveryByIdAsync(deliveryId);

                if (delivery == null)
                {
                    Log.Warning("Message delivery with ID: {DeliveryId} not found.", deliveryId);
                    return Results.NotFound(new { message = "Delivery not found." });
                }

                Log.Information("Successfully retrieved message delivery with ID: {DeliveryId}", deliveryId);
                return Results.Ok(delivery);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the message delivery.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> MarkDeliveryAsAttemptedAsync(
            IMessageDeliveriesRepository repo,
            int deliveryId,
            [FromBody] string providerResponse,
            string? providerMessageId = null)
        {
            try
            {
                Log.Information("Attempting to mark delivery as attempted for Delivery ID: {DeliveryId}", deliveryId);

                // Mark delivery as attempted
                var result = await repo.MarkDeliveryAsAttemptedAsync(deliveryId, providerResponse, providerMessageId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to mark delivery as attempted." });
                }

                Log.Information("Successfully marked delivery as attempted with Delivery ID: {DeliveryId}", deliveryId);
                return Results.Ok(new { message = "Delivery marked as attempted successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while marking the delivery as attempted.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> MarkDeliveryAsDeliveredAsync(
            IMessageDeliveriesRepository repo,
            int deliveryId)
        {
            try
            {
                Log.Information("Attempting to mark delivery as delivered for Delivery ID: {DeliveryId}", deliveryId);

                // Mark delivery as delivered
                var result = await repo.MarkDeliveryAsDeliveredAsync(deliveryId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to mark delivery as delivered." });
                }

                Log.Information("Successfully marked delivery as delivered with Delivery ID: {DeliveryId}", deliveryId);
                return Results.Ok(new { message = "Delivery marked as delivered successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while marking the delivery as delivered.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> MarkDeliveryAsFailedAsync(
            IMessageDeliveriesRepository repo,
            int deliveryId,
            [FromBody] string failureReason)
        {
            try
            {
                Log.Information("Attempting to mark delivery as failed for Delivery ID: {DeliveryId}", deliveryId);

                // Mark delivery as failed
                var result = await repo.MarkDeliveryAsFailedAsync(deliveryId, failureReason);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to mark delivery as failed." });
                }

                Log.Information("Successfully marked delivery as failed with Delivery ID: {DeliveryId}", deliveryId);
                return Results.Ok(new { message = "Delivery marked as failed successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while marking the delivery as failed.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> RetryDeliveryAsync(
            IMessageDeliveriesRepository repo,
            int deliveryId)
        {
            try
            {
                Log.Information("Attempting to retry delivery for Delivery ID: {DeliveryId}", deliveryId);

                // Retry delivery
                var result = await repo.RetryDeliveryAsync(deliveryId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to retry delivery." });
                }

                Log.Information("Successfully retried delivery with Delivery ID: {DeliveryId}", deliveryId);
                return Results.Ok(new { message = "Delivery retry initiated successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrying the delivery.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateDeliveryStatusAsync(
            IMessageDeliveriesRepository repo,
            int deliveryId,
            [FromBody] string status)
        {
            try
            {
                Log.Information("Attempting to update status for Delivery ID: {DeliveryId} to {Status}", deliveryId, status);

                // Update delivery status
                var result = await repo.UpdateDeliveryStatusAsync(deliveryId, status);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to update delivery status." });
                }

                Log.Information("Successfully updated delivery status with Delivery ID: {DeliveryId}", deliveryId);
                return Results.Ok(new { message = "Delivery status updated successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the delivery status.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetDeliveryStatusDistributionAsync(
            IMessageDeliveriesRepository repo)
        {
            try
            {
                Log.Information("Attempting to retrieve delivery status distribution.");

                // Retrieve delivery status distribution
                var statusDistribution = await repo.GetDeliveryStatusDistributionAsync();

                Log.Information("Successfully retrieved delivery status distribution.");
                return Results.Ok(statusDistribution);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the delivery status distribution.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CleanupOldDeliveriesAsync(
            IMessageDeliveriesRepository repo,
            DateTime cutoffDate)
        {
            try
            {
                Log.Information("Attempting to cleanup old deliveries before {CutoffDate}.", cutoffDate);

                // Cleanup old deliveries
                var cleanedUpCount = await repo.CleanupOldDeliveriesAsync(cutoffDate);

                Log.Information("Successfully cleaned up {CleanedUpCount} deliveries.", cleanedUpCount);
                return Results.Ok(new { message = $"{cleanedUpCount} old deliveries cleaned up successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while cleaning up old deliveries.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
