using Api.Common.Abstractions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Application.NotificationRequests.Abstractions;
using Domain.NotificationRequests.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Api.NotificationRequests.Controllers;

namespace Api.NotificationRequests.EndPointDefinitions
{
    public class NotificationRequestsEndpoints : IEndpointDefinition
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

            var notificationRequests = versionedGroup.MapGroup("/notificationrequests")
                .WithTags("Notification Requests Management");

            // Create a new notification request
            notificationRequests.MapPost("/", async (
                INotificationRequestsRepository repo,
                [FromBody] NotificationRequestCreationRequest request) =>
            {
                return await NotificationRequestsController.CreateRequestAsync(repo, request);
            });

            // Get all notification requests (paginated with filters)
            notificationRequests.MapGet("/", async (
                INotificationRequestsRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] int? applicationId = null,
                [FromQuery] int? templateId = null,
                [FromQuery] string? status = null,
                [FromQuery] string? priority = null,
                [FromQuery] DateTime? fromDate = null,
                [FromQuery] DateTime? toDate = null,
                [FromQuery] int? requestedByUserId = null) =>
            {
                return await NotificationRequestsController.GetRequestsAsync(
                    repo, pageNumber, pageSize, applicationId, templateId,
                    status, priority, fromDate, toDate, requestedByUserId);
            });

            // Get a specific request by ID
            notificationRequests.MapGet("/{requestId:guid}", async (
                INotificationRequestsRepository repo,
                Guid requestId) =>
            {
                return await NotificationRequestsController.GetRequestByIdAsync(repo, requestId);
            });

            // Mark request as processing
            notificationRequests.MapPut("/{requestId:guid}/processing", async (
                INotificationRequestsRepository repo,
                Guid requestId) =>
            {
                return await NotificationRequestsController.MarkRequestAsProcessingAsync(repo, requestId);
            });

            // Mark request as completed
            notificationRequests.MapPut("/{requestId:guid}/completed", async (
                INotificationRequestsRepository repo,
                Guid requestId) =>
            {
                return await NotificationRequestsController.MarkRequestAsCompletedAsync(repo, requestId);
            });

            // Mark request as failed
            notificationRequests.MapPut("/{requestId:guid}/failed", async (
                INotificationRequestsRepository repo,
                Guid requestId,
                [FromBody] string errorDetails) =>
            {
                return await NotificationRequestsController.MarkRequestAsFailedAsync(repo, requestId, errorDetails);
            });

            // Cancel a request
            notificationRequests.MapPut("/{requestId:guid}/cancel", async (
                INotificationRequestsRepository repo,
                Guid requestId) =>
            {
                return await NotificationRequestsController.CancelRequestAsync(repo, requestId);
            });

            // Update request status
            notificationRequests.MapPut("/{requestId:guid}/status", async (
                INotificationRequestsRepository repo,
                Guid requestId,
                [FromBody] string status) =>
            {
                return await NotificationRequestsController.UpdateRequestStatusAsync(repo, requestId, status);
            });

            // Update request priority
            notificationRequests.MapPut("/{requestId:guid}/priority", async (
                INotificationRequestsRepository repo,
                Guid requestId,
                [FromBody] string priority) =>
            {
                return await NotificationRequestsController.UpdateRequestPriorityAsync(repo, requestId, priority);
            });

            // Get expired requests
            notificationRequests.MapGet("/expired", async (
                INotificationRequestsRepository repo) =>
            {
                return await NotificationRequestsController.GetExpiredRequestsAsync(repo);
            });

            // Update request data
            notificationRequests.MapPut("/{requestId:guid}/data", async (
                INotificationRequestsRepository repo,
                Guid requestId,
                [FromBody] string requestDataJson) =>
            {
                return await NotificationRequestsController.UpdateRequestDataAsync(repo, requestId, requestDataJson);
            });

            //// Bulk update status
            //notificationRequests.MapPut("/bulk/status", async (
            //    INotificationRequestsRepository repo,
            //    [FromBody] IEnumerable<Guid> requestIds,
            //    [FromBody] string status) =>
            //{
            //    return await NotificationRequestsController.BulkUpdateStatusAsync(repo, requestIds, status);
            //});

            // Bulk cancel requests
            notificationRequests.MapPut("/bulk/cancel", async (
                INotificationRequestsRepository repo,
                [FromBody] IEnumerable<Guid> requestIds) =>
            {
                return await NotificationRequestsController.BulkCancelRequestsAsync(repo, requestIds);
            });
        }
    }
}