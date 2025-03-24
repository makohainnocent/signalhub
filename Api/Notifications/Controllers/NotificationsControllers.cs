// Api/Notifications/Controllers/NotificationsControllers.cs
using Domain.Notifications.Models;
using Domain.Notifications.Requests;
using Application.Notifications.Abstractions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Claims;
using DataAccess.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Notifications.Controllers
{
    public static class NotificationsControllers
    {
        public static async Task<IResult> CreateNotificationAsync(
            INotificationsRepository repo,
            [FromBody] NotificationCreationRequest request,
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

                Log.Information("Attempting to create notification for user ID: {UserId}", request.UserId);

                // Create the notification
                var createdNotification = await repo.CreateNotificationAsync(request);

                Log.Information("Notification created successfully with ID: {NotificationId}", createdNotification.NotificationId);
                return Results.Created($"/notifications/{createdNotification.NotificationId}", createdNotification);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the notification.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetNotificationsAsync(
            INotificationsRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            int? userId=null,
            string? status=null)
        {
            try
            {
                Log.Information("Attempting to retrieve notifications with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                // Retrieve paginated notifications
                var pagedResult = await repo.GetNotificationsAsync(pageNumber, pageSize, search,userId,status);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No notifications found." });
                }

                Log.Information("Successfully retrieved {NotificationCount} notifications out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving notifications.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetNotificationByIdAsync(
            INotificationsRepository repo,
            int notificationId)
        {
            try
            {
                Log.Information("Attempting to retrieve notification with ID: {NotificationId}", notificationId);

                // Retrieve the notification by ID
                var notification = await repo.GetNotificationByIdAsync(notificationId);

                if (notification == null)
                {
                    Log.Warning("Notification with ID: {NotificationId} not found.", notificationId);
                    return Results.NotFound(new { message = "Notification not found." });
                }

                Log.Information("Successfully retrieved notification with ID: {NotificationId}", notificationId);
                return Results.Ok(notification);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the notification.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateNotificationAsync(
            INotificationsRepository repo,
            [FromBody] NotificationUpdateRequest request)
        {
            try
            {
                Log.Information("Attempting to update notification with ID: {NotificationId}", request.NotificationId);

                // Update the notification
                var updatedNotification = await repo.UpdateNotificationAsync(request);

                Log.Information("Notification updated successfully with ID: {NotificationId}", updatedNotification.NotificationId);
                return Results.Ok(updatedNotification);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Notification update failed - notification does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the notification.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteNotificationAsync(
            INotificationsRepository repo,
            int notificationId)
        {
            try
            {
                Log.Information("Attempting to delete notification with ID: {NotificationId}", notificationId);

                // Delete the notification
                var deleted = await repo.DeleteNotificationAsync(notificationId);

                if (deleted)
                {
                    Log.Information("Notification deleted successfully with ID: {NotificationId}", notificationId);
                    return Results.Ok(new { message = "Notification deleted successfully." });
                }
                else
                {
                    Log.Warning("Notification deletion failed - notification does not exist.");
                    return Results.NotFound(new { message = "Notification not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the notification.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountNotificationsAsync(
            INotificationsRepository repo)
        {
            try
            {
                Log.Information("Attempting to count all notifications.");

                // Count the notifications
                var count = await repo.CountNotificationsAsync();

                Log.Information("Successfully counted {NotificationCount} notifications.", count);
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting notifications.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> SendSms(HttpContext httpContext, string phoneNumber, string msg)
        {
            try
            {
                Log.Information("Attempting to send SMS to {PhoneNumber}.", phoneNumber);

                // Get SmsService from dependency injection
                var smsService = httpContext.RequestServices.GetRequiredService<Api.Core.Services.SmsService>();

                // Call the SendSmsAsync method
                bool isSent = await smsService.SendSmsAsync(phoneNumber, msg);

                if (isSent)
                {
                    Log.Information("SMS sent successfully to {PhoneNumber}.", phoneNumber);
                    return Results.Ok(new { message = "SMS sent successfully" });
                }
                else
                {
                    Log.Warning("Failed to send SMS to {PhoneNumber}.", phoneNumber);
                    return Results.Problem("Failed to send SMS");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while sending SMS.");
                return Results.Problem(ex.Message);
            }
        }
    }
}