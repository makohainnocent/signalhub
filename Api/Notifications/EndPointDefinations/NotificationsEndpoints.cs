// Api/Notifications/EndPointDefinations/NotificationsEndpoints.cs
using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.Notifications.Abstractions;
using Domain.Notifications.Requests;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Api.Notifications.Controllers;

namespace Api.Notifications.EndPointDefinations
{
    public class NotificationsEndpoints : IEndpointDefinition
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

            var notifications = versionedGroup.MapGroup("/notifications")
                .WithTags("Notifications Management");

            // Create a new notification
            notifications.MapPost("/", async (INotificationsRepository repo, [FromBody] NotificationCreationRequest request, HttpContext httpContext) =>
            {
                return await NotificationsControllers.CreateNotificationAsync(repo, request, httpContext);
            })
            .RequireAuthorization();

            // Get all notifications (paginated)
            notifications.MapGet("/", async (INotificationsRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] int? userId = null, [FromQuery] string? status = null) =>
            {
                return await NotificationsControllers.GetNotificationsAsync(repo, pageNumber, pageSize, search,userId,status);
            });

            // Get a notification by ID
            notifications.MapGet("/{notificationId:int}", async (INotificationsRepository repo, int notificationId) =>
            {
                return await NotificationsControllers.GetNotificationByIdAsync(repo, notificationId);
            });

            // Update a notification
            notifications.MapPut("/", async (INotificationsRepository repo, [FromBody] NotificationUpdateRequest request) =>
            {
                return await NotificationsControllers.UpdateNotificationAsync(repo, request);
            })
            .RequireAuthorization();

            // Delete a notification
            notifications.MapDelete("/{notificationId:int}", async (INotificationsRepository repo, int notificationId) =>
            {
                return await NotificationsControllers.DeleteNotificationAsync(repo, notificationId);
            })
            .RequireAuthorization();

            // Count all notifications
            notifications.MapGet("/count", async (INotificationsRepository repo) =>
            {
                return await NotificationsControllers.CountNotificationsAsync(repo);
            });


            notifications.MapGet("/sendSms", async (HttpContext context, [FromQuery] string phoneNumber, [FromQuery] string msg) =>
            {
                return await NotificationsControllers.SendSms(context, phoneNumber, msg);
            });

        }
    }
}