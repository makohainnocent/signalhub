using Domain.NotificationRequests;
using Domain.NotificationRequests.Requests;
using Application.NotificationRequests.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.NotificationRequests.Controllers
{
    public static class NotificationRequestsController
    {
        public static async Task<IResult> CreateRequestAsync(
            INotificationRequestsRepository repo,
            [FromBody] NotificationRequestCreationRequest request)
        {
            try
            {
                Log.Information("Attempting to create notification request.");

                // Create the request
                var notificationRequest = await repo.CreateRequestAsync(request);

                Log.Information("Notification request created successfully with Request ID: {RequestId}", notificationRequest.RequestId);
                return Results.Created($"/notificationrequests/{notificationRequest.RequestId}", notificationRequest);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the notification request.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetRequestsAsync(
            INotificationRequestsRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            int? applicationId = null,
            int? templateId = null,
            string? status = null,
            string? priority = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? requestedByUserId = null)
        {
            try
            {
                Log.Information("Attempting to retrieve notification requests with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                // Retrieve paginated requests
                var pagedResult = await repo.GetRequestsAsync(pageNumber, pageSize, applicationId, templateId, status, priority, fromDate, toDate, requestedByUserId);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No requests found." });
                }

                Log.Information("Successfully retrieved {RequestCount} requests out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving notification requests.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetRequestByIdAsync(
            INotificationRequestsRepository repo,
            Guid requestId)
        {
            try
            {
                Log.Information("Attempting to retrieve notification request with Request ID: {RequestId}", requestId);

                // Retrieve the request by ID
                var request = await repo.GetRequestByIdAsync(requestId);

                if (request == null)
                {
                    Log.Warning("Notification request with Request ID: {RequestId} not found.", requestId);
                    return Results.NotFound(new { message = "Request not found." });
                }

                Log.Information("Successfully retrieved notification request with Request ID: {RequestId}", requestId);
                return Results.Ok(request);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the notification request.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> MarkRequestAsProcessingAsync(
            INotificationRequestsRepository repo,
            Guid requestId)
        {
            try
            {
                Log.Information("Attempting to mark notification request as processing with Request ID: {RequestId}", requestId);

                // Mark the request as processing
                var result = await repo.MarkRequestAsProcessingAsync(requestId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to mark request as processing." });
                }

                Log.Information("Successfully marked notification request as processing with Request ID: {RequestId}", requestId);
                return Results.Ok(new { message = "Request marked as processing." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while marking the notification request as processing.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> MarkRequestAsCompletedAsync(
            INotificationRequestsRepository repo,
            Guid requestId)
        {
            try
            {
                Log.Information("Attempting to mark notification request as completed with Request ID: {RequestId}", requestId);

                // Mark the request as completed
                var result = await repo.MarkRequestAsCompletedAsync(requestId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to mark request as completed." });
                }

                Log.Information("Successfully marked notification request as completed with Request ID: {RequestId}", requestId);
                return Results.Ok(new { message = "Request marked as completed." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while marking the notification request as completed.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> MarkRequestAsFailedAsync(
            INotificationRequestsRepository repo,
            Guid requestId,
            [FromBody] string errorDetails)
        {
            try
            {
                Log.Information("Attempting to mark notification request as failed with Request ID: {RequestId}", requestId);

                // Mark the request as failed
                var result = await repo.MarkRequestAsFailedAsync(requestId, errorDetails);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to mark request as failed." });
                }

                Log.Information("Successfully marked notification request as failed with Request ID: {RequestId}", requestId);
                return Results.Ok(new { message = "Request marked as failed." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while marking the notification request as failed.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CancelRequestAsync(
            INotificationRequestsRepository repo,
            Guid requestId)
        {
            try
            {
                Log.Information("Attempting to cancel notification request with Request ID: {RequestId}", requestId);

                // Cancel the request
                var result = await repo.CancelRequestAsync(requestId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to cancel request." });
                }

                Log.Information("Successfully canceled notification request with Request ID: {RequestId}", requestId);
                return Results.Ok(new { message = "Request canceled." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while canceling the notification request.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateRequestStatusAsync(
            INotificationRequestsRepository repo,
            Guid requestId,
            [FromBody] string status)
        {
            try
            {
                Log.Information("Attempting to update status for notification request with Request ID: {RequestId} to {Status}", requestId, status);

                // Update the status of the request
                var result = await repo.UpdateRequestStatusAsync(requestId, status);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to update request status." });
                }

                Log.Information("Successfully updated status for notification request with Request ID: {RequestId}", requestId);
                return Results.Ok(new { message = "Request status updated." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the notification request status.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateRequestPriorityAsync(
            INotificationRequestsRepository repo,
            Guid requestId,
            [FromBody] string priority)
        {
            try
            {
                Log.Information("Attempting to update priority for notification request with Request ID: {RequestId} to {Priority}", requestId, priority);

                // Update the priority of the request
                var result = await repo.UpdateRequestPriorityAsync(requestId, priority);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to update request priority." });
                }

                Log.Information("Successfully updated priority for notification request with Request ID: {RequestId}", requestId);
                return Results.Ok(new { message = "Request priority updated." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the notification request priority.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetExpiredRequestsAsync(
            INotificationRequestsRepository repo)
        {
            try
            {
                Log.Information("Attempting to retrieve expired notification requests.");

                // Retrieve expired requests
                var expiredRequests = await repo.GetExpiredRequestsAsync();

                if (!expiredRequests.Any())
                {
                    return Results.NotFound(new { message = "No expired requests found." });
                }

                Log.Information("Successfully retrieved {ExpiredRequestCount} expired requests.", expiredRequests.Count);
                return Results.Ok(expiredRequests);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving expired notification requests.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateRequestDataAsync(
            INotificationRequestsRepository repo,
            Guid requestId,
            [FromBody] string requestDataJson)
        {
            try
            {
                Log.Information("Attempting to update request data for notification request with Request ID: {RequestId}", requestId);

                // Update the request data
                var result = await repo.UpdateRequestDataAsync(requestId, requestDataJson);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to update request data." });
                }

                Log.Information("Successfully updated request data for notification request with Request ID: {RequestId}", requestId);
                return Results.Ok(new { message = "Request data updated." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the request data.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> BulkUpdateStatusAsync(
            INotificationRequestsRepository repo,
            [FromBody] IEnumerable<Guid> requestIds,
            [FromBody] string status)
        {
            try
            {
                Log.Information("Attempting to update status for {RequestCount} notification requests to {Status}.", requestIds.Count(), status);

                // Bulk update status
                var updatedCount = await repo.BulkUpdateStatusAsync(requestIds, status);

                Log.Information("Successfully updated status for {UpdatedCount} notification requests.", updatedCount);
                return Results.Ok(new { message = $"{updatedCount} notification requests' status updated." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while bulk updating the status of notification requests.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> BulkCancelRequestsAsync(
            INotificationRequestsRepository repo,
            [FromBody] IEnumerable<Guid> requestIds)
        {
            try
            {
                Log.Information("Attempting to cancel {RequestCount} notification requests.", requestIds.Count());

                // Bulk cancel requests
                var canceledCount = await repo.BulkCancelRequestsAsync(requestIds);

                Log.Information("Successfully canceled {CanceledCount} notification requests.", canceledCount);
                return Results.Ok(new { message = $"{canceledCount} notification requests canceled." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while bulk canceling notification requests.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
