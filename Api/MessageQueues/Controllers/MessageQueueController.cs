
using Domain.MessageQueues.Requests;
using Application.MessageQueue.Abstractions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using DataAccess.Common.Exceptions;

namespace Api.MessageQueues.Controllers
{
    public static class MessageQueueController
    {
        public static async Task<IResult> EnqueueMessageAsync(
            IMessageQueueRepository repo,
            [FromBody] QueuedMessageCreationRequest request,
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

                Log.Information("Attempting to enqueue message for user ID: {UserId}", userId);

                // Enqueue the message
                var queuedMessage = await repo.EnqueueMessageAsync(request);

                Log.Information("Message enqueued successfully with Queue ID: {QueueId}", queuedMessage.QueueId);
                return Results.Created($"/messagequeues/{queuedMessage.QueueId}", queuedMessage);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while enqueuing the message.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DequeueMessageAsync(
            IMessageQueueRepository repo)
        {
            try
            {
                Log.Information("Attempting to dequeue a message.");

                // Dequeue a message
                var dequeuedMessage = await repo.DequeueMessageAsync();

                if (dequeuedMessage == null)
                {
                    return Results.NotFound(new { message = "No messages available in the queue." });
                }

                Log.Information("Message dequeued successfully with Queue ID: {QueueId}", dequeuedMessage.QueueId);
                return Results.Ok(dequeuedMessage);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while dequeuing the message.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetQueuedMessagesAsync(
            IMessageQueueRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            Guid? requestId = null,
            int? recipientId = null,
            string? channelType = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool? highPriorityFirst = true)
        {
            try
            {
                Log.Information("Attempting to retrieve queued messages with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                // Retrieve paginated queued messages
                var pagedResult = await repo.GetQueuedMessagesAsync(pageNumber, pageSize, requestId, recipientId, channelType, status, fromDate, toDate, highPriorityFirst);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No messages found in the queue." });
                }

                Log.Information("Successfully retrieved {MessageCount} queued messages out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving queued messages.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetMessageByIdAsync(
            IMessageQueueRepository repo,
            long queueId)
        {
            try
            {
                Log.Information("Attempting to retrieve message with Queue ID: {QueueId}", queueId);

                // Retrieve the message by ID
                var message = await repo.GetMessageByIdAsync(queueId);

                if (message == null)
                {
                    Log.Warning("Message with Queue ID: {QueueId} not found.", queueId);
                    return Results.NotFound(new { message = "Message not found." });
                }

                Log.Information("Successfully retrieved message with Queue ID: {QueueId}", queueId);
                return Results.Ok(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the message.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> MarkMessageAsProcessingAsync(
            IMessageQueueRepository repo,
            long queueId)
        {
            try
            {
                Log.Information("Attempting to mark message as processing with Queue ID: {QueueId}", queueId);

                // Mark message as processing
                var result = await repo.MarkMessageAsProcessingAsync(queueId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to mark message as processing." });
                }

                Log.Information("Successfully marked message as processing with Queue ID: {QueueId}", queueId);
                return Results.Ok(new { message = "Message marked as processing." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while marking the message as processing.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> MarkMessageAsCompletedAsync(
            IMessageQueueRepository repo,
            long queueId)
        {
            try
            {
                Log.Information("Attempting to mark message as completed with Queue ID: {QueueId}", queueId);

                // Mark message as completed
                var result = await repo.MarkMessageAsCompletedAsync(queueId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to mark message as completed." });
                }

                Log.Information("Successfully marked message as completed with Queue ID: {QueueId}", queueId);
                return Results.Ok(new { message = "Message marked as completed." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while marking the message as completed.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> MarkMessageAsFailedAsync(
            IMessageQueueRepository repo,
            long queueId,
            [FromBody] string errorDetails)
        {
            try
            {
                Log.Information("Attempting to mark message as failed with Queue ID: {QueueId}", queueId);

                // Mark message as failed
                var result = await repo.MarkMessageAsFailedAsync(queueId, errorDetails);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to mark message as failed." });
                }

                Log.Information("Successfully marked message as failed with Queue ID: {QueueId}", queueId);
                return Results.Ok(new { message = "Message marked as failed." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while marking the message as failed.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateMessageStatusAsync(
            IMessageQueueRepository repo,
            long queueId,
            [FromBody] string status)
        {
            try
            {
                Log.Information("Attempting to update status for message with Queue ID: {QueueId} to {Status}", queueId, status);

                // Update message status
                var result = await repo.UpdateMessageStatusAsync(queueId, status);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to update message status." });
                }

                Log.Information("Successfully updated message status with Queue ID: {QueueId}", queueId);
                return Results.Ok(new { message = "Message status updated." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the message status.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetQueueStatusAsync(
            IMessageQueueRepository repo)
        {
            try
            {
                Log.Information("Attempting to retrieve queue status.");

                // Retrieve queue status
                var queueStatus = await repo.GetQueueStatusAsync();

                Log.Information("Successfully retrieved queue status.");
                return Results.Ok(queueStatus);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the queue status.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> PurgeProcessedMessagesAsync(
            IMessageQueueRepository repo,
            DateTime olderThan)
        {
            try
            {
                Log.Information("Attempting to purge processed messages older than {OlderThan}.", olderThan);

                // Purge processed messages
                var purgedCount = await repo.PurgeProcessedMessagesAsync(olderThan);

                Log.Information("Successfully purged {PurgedCount} processed messages.", purgedCount);
                return Results.Ok(new { message = $"{purgedCount} processed messages purged." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while purging processed messages.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
