using Api.Common.Abstractions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Application.MessageDeliveries.Abstractions;
using Domain.MessageDeliveries.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Api.MessageDeliveries.Controllers;

namespace Api.MessageDeliveries.EndPointDefinitions
{
    public class MessageDeliveriesEndpoints : IEndpointDefinition
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

            var messageDeliveries = versionedGroup.MapGroup("/messagedeliveries")
                .WithTags("Message Deliveries Management");

            // Create a new message delivery
            messageDeliveries.MapPost("/", async (
                IMessageDeliveriesRepository repo,
                [FromBody] MessageDeliveryCreationRequest request,
                HttpContext httpContext) =>
            {
                return await MessageDeliveriesController.CreateDeliveryAsync(repo, request, httpContext);
            })
            .RequireAuthorization();

            // Get all deliveries (paginated with filters)
            messageDeliveries.MapGet("/", async (
                IMessageDeliveriesRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] Guid? requestId = null,
                [FromQuery] int? recipientId = null,
                [FromQuery] int? providerId = null,
                [FromQuery] string? channelType = null,
                [FromQuery] string? status = null,
                [FromQuery] DateTime? fromDate = null,
                [FromQuery] DateTime? toDate = null,
                [FromQuery] bool? onlyFailed = false) =>
            {
                return await MessageDeliveriesController.GetDeliveriesAsync(
                    repo, pageNumber, pageSize, requestId, recipientId, providerId,
                    channelType, status, fromDate, toDate, onlyFailed);
            });

            // Get a specific delivery by ID
            messageDeliveries.MapGet("/{deliveryId:int}", async (
                IMessageDeliveriesRepository repo,
                int deliveryId) =>
            {
                return await MessageDeliveriesController.GetDeliveryByIdAsync(repo, deliveryId);
            });

            // Mark delivery as attempted
            messageDeliveries.MapPut("/{deliveryId:int}/attempted", async (
                IMessageDeliveriesRepository repo,
                int deliveryId,
                [FromBody] string providerResponse,
                [FromQuery] string? providerMessageId = null) =>
            {
                return await MessageDeliveriesController.MarkDeliveryAsAttemptedAsync(
                    repo, deliveryId, providerResponse, providerMessageId);
            });

            // Mark delivery as delivered
            messageDeliveries.MapPut("/{deliveryId:int}/delivered", async (
                IMessageDeliveriesRepository repo,
                int deliveryId) =>
            {
                return await MessageDeliveriesController.MarkDeliveryAsDeliveredAsync(repo, deliveryId);
            });

            // Mark delivery as failed
            messageDeliveries.MapPut("/{deliveryId:int}/failed", async (
                IMessageDeliveriesRepository repo,
                int deliveryId,
                [FromBody] string failureReason) =>
            {
                return await MessageDeliveriesController.MarkDeliveryAsFailedAsync(repo, deliveryId, failureReason);
            });

            // Retry a failed delivery
            messageDeliveries.MapPost("/{deliveryId:int}/retry", async (
                IMessageDeliveriesRepository repo,
                int deliveryId) =>
            {
                return await MessageDeliveriesController.RetryDeliveryAsync(repo, deliveryId);
            });

            // Update delivery status
            messageDeliveries.MapPut("/{deliveryId:int}/status", async (
                IMessageDeliveriesRepository repo,
                int deliveryId,
                [FromBody] string status) =>
            {
                return await MessageDeliveriesController.UpdateDeliveryStatusAsync(repo, deliveryId, status);
            });

            // Get delivery status distribution
            messageDeliveries.MapGet("/status-distribution", async (
                IMessageDeliveriesRepository repo) =>
            {
                return await MessageDeliveriesController.GetDeliveryStatusDistributionAsync(repo);
            });

            // Cleanup old deliveries
            messageDeliveries.MapDelete("/cleanup", async (
                IMessageDeliveriesRepository repo,
                [FromQuery] DateTime cutoffDate) =>
            {
                return await MessageDeliveriesController.CleanupOldDeliveriesAsync(repo, cutoffDate);
            });
        }
    }
}