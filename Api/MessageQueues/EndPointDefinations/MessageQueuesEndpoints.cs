using Api.Common.Abstractions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Application.MessageQueue.Abstractions;
using Domain.MessageQueues.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Api.MessageQueues.Controllers;

namespace Api.MessageQueues.EndPointDefinitions
{
    public class MessageQueuesEndpoints : IEndpointDefinition
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

            var messageQueues = versionedGroup.MapGroup("/messagequeues")
                .WithTags("Message Queues Management");

            // Enqueue a new message
            messageQueues.MapPost("/", async (
                IMessageQueueRepository repo,
                [FromBody] QueuedMessageCreationRequest request,
                HttpContext httpContext) =>
            {
                return await MessageQueueController.EnqueueMessageAsync(repo, request, httpContext);
            })
            .RequireAuthorization();

            // Dequeue the next message
            messageQueues.MapPost("/dequeue", async (
                IMessageQueueRepository repo) =>
            {
                return await MessageQueueController.DequeueMessageAsync(repo);
            });

            // Get queued messages (paginated with filters)
            messageQueues.MapGet("/", async (
                IMessageQueueRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] Guid? requestId = null,
                [FromQuery] int? recipientId = null,
                [FromQuery] string? channelType = null,
                [FromQuery] string? status = null,
                [FromQuery] DateTime? fromDate = null,
                [FromQuery] DateTime? toDate = null,
                [FromQuery] bool? highPriorityFirst = true) =>
            {
                return await MessageQueueController.GetQueuedMessagesAsync(
                    repo, pageNumber, pageSize, requestId, recipientId,
                    channelType, status, fromDate, toDate, highPriorityFirst);
            });

            // Get a specific message by ID
            messageQueues.MapGet("/{queueId:long}", async (
                IMessageQueueRepository repo,
                long queueId) =>
            {
                return await MessageQueueController.GetMessageByIdAsync(repo, queueId);
            });

            // Mark message as processing
            messageQueues.MapPut("/{queueId:long}/processing", async (
                IMessageQueueRepository repo,
                long queueId) =>
            {
                return await MessageQueueController.MarkMessageAsProcessingAsync(repo, queueId);
            });

            // Mark message as completed
            messageQueues.MapPut("/{queueId:long}/completed", async (
                IMessageQueueRepository repo,
                long queueId) =>
            {
                return await MessageQueueController.MarkMessageAsCompletedAsync(repo, queueId);
            });

            // Mark message as failed
            messageQueues.MapPut("/{queueId:long}/failed", async (
                IMessageQueueRepository repo,
                long queueId,
                [FromBody] string errorDetails) =>
            {
                return await MessageQueueController.MarkMessageAsFailedAsync(repo, queueId, errorDetails);
            });

            // Update message status
            messageQueues.MapPut("/{queueId:long}/status", async (
                IMessageQueueRepository repo,
                long queueId,
                [FromBody] string status) =>
            {
                return await MessageQueueController.UpdateMessageStatusAsync(repo, queueId, status);
            });

            // Get queue status
            messageQueues.MapGet("/status", async (
                IMessageQueueRepository repo) =>
            {
                return await MessageQueueController.GetQueueStatusAsync(repo);
            });

            // Purge processed messages
            messageQueues.MapDelete("/purge", async (
                IMessageQueueRepository repo,
                [FromQuery] DateTime olderThan) =>
            {
                return await MessageQueueController.PurgeProcessedMessagesAsync(repo, olderThan);
            });
        }
    }
}