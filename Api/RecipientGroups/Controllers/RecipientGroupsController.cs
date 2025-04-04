using Application.RecipientGroups.Abstractions;
using Domain.Common.Responses;
using Domain.RecipientGroups;
using Domain.RecipientGroups.Requests;
using Domain.Recipients;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Api.RecipientGroups.Controllers
{

    [ApiController]
    public static class RecipientGroupsController
    {
        // Create a new recipient group
        public static async Task<IResult> CreateRecipientGroupAsync(
            IRecipientGroupsRepository repo,
            [FromBody] RecipientGroupCreationRequest request)
        {
            try
            {
                Log.Information("Creating a new recipient group with name {GroupName}", request.Name);

                var group = await repo.CreateRecipientGroupAsync(request);

                return Results.Created($"/api/RecipientGroups/{group.GroupId}", group);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating recipient group.");
                return Results.Problem(ex.Message);
            }
        }

        // Get recipient groups with pagination
        public static async Task<IResult> GetRecipientGroupsAsync(
            IRecipientGroupsRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            int? tenantId = null,
            int? createdByUserId = null)
        {
            try
            {
                Log.Information("Fetching recipient groups with pagination: Page {PageNumber}, PageSize {PageSize}", pageNumber, pageSize);

                var groups = await repo.GetRecipientGroupsAsync(pageNumber, pageSize, search, tenantId, createdByUserId);

                return Results.Ok(groups);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching recipient groups.");
                return Results.Problem(ex.Message);
            }
        }

        // Get a recipient group by ID
        public static async Task<IResult> GetRecipientGroupByIdAsync(
            IRecipientGroupsRepository repo,
            int groupId)
        {
            try
            {
                Log.Information("Fetching recipient group with ID {GroupId}", groupId);

                var group = await repo.GetRecipientGroupByIdAsync(groupId);
                if (group == null)
                {
                    return Results.NotFound(new { message = "Group not found." });
                }

                return Results.Ok(group);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching recipient group.");
                return Results.Problem(ex.Message);
            }
        }

        // Update a recipient group
        public static async Task<IResult> UpdateRecipientGroupAsync(
            IRecipientGroupsRepository repo,
            [FromBody] RecipientGroupUpdateRequest request)
        {
            try
            {
                Log.Information("Updating recipient group with ID {GroupId}", request.GroupId);

                var updatedGroup = await repo.UpdateRecipientGroupAsync(request);
                return Results.Ok(updatedGroup);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating recipient group.");
                return Results.Problem(ex.Message);
            }
        }

        // Delete a recipient group
        public static async Task<IResult> DeleteRecipientGroupAsync(
            IRecipientGroupsRepository repo,
            int groupId)
        {
            try
            {
                Log.Information("Deleting recipient group with ID {GroupId}", groupId);

                var success = await repo.DeleteRecipientGroupAsync(groupId);
                if (!success)
                {
                    return Results.BadRequest(new { message = "Failed to delete recipient group." });
                }

                return Results.Ok(new { message = "Recipient group deleted successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting recipient group.");
                return Results.Problem(ex.Message);
            }
        }

        // Count the number of recipient groups
        public static async Task<IResult> CountRecipientGroupsAsync(IRecipientGroupsRepository repo)
        {
            try
            {
                Log.Information("Counting recipient groups");

                var count = await repo.CountRecipientGroupsAsync();
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error counting recipient groups.");
                return Results.Problem(ex.Message);
            }
        }

        // Add a recipient to a group
        public static async Task<IResult> AddRecipientToGroupAsync(
            IRecipientGroupsRepository repo,
            int groupId,
            int recipientId,
            HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                Log.Information("Adding recipient {RecipientId} to group {GroupId} by user {UserId}", recipientId, groupId, userId);

                var success = await repo.AddRecipientToGroupAsync(groupId, recipientId, userId);
                if (!success)
                {
                    return Results.BadRequest(new { message = "Failed to add recipient to group." });
                }

                return Results.Ok(new { message = "Recipient added successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding recipient to group.");
                return Results.Problem(ex.Message);
            }
        }

        // Remove a recipient from a group
        public static async Task<IResult> RemoveRecipientFromGroupAsync(
            IRecipientGroupsRepository repo,
            int groupId,
            int recipientId)
        {
            try
            {
                Log.Information("Removing recipient {RecipientId} from group {GroupId}", recipientId, groupId);

                var success = await repo.RemoveRecipientFromGroupAsync(groupId, recipientId);
                if (!success)
                {
                    return Results.BadRequest(new { message = "Failed to remove recipient from group." });
                }

                return Results.Ok(new { message = "Recipient removed successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error removing recipient from group.");
                return Results.Problem(ex.Message);
            }
        }

        // Get recipients of a group
        public static async Task<IResult> GetGroupRecipientsAsync(
            IRecipientGroupsRepository repo,
            int groupId,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            try
            {
                Log.Information("Fetching recipients of group {GroupId}", groupId);

                var recipients = await repo.GetGroupRecipientsAsync(groupId, pageNumber, pageSize, search);
                if (recipients.Items == null || !recipients.Items.Any())
                {
                    return Results.NotFound(new { message = "No recipients found in the group." });
                }

                return Results.Ok(recipients);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching group recipients.");
                return Results.Problem(ex.Message);
            }
        }

        // Count recipients in a group
        public static async Task<IResult> CountGroupRecipientsAsync(
            IRecipientGroupsRepository repo,
            int groupId)
        {
            try
            {
                Log.Information("Counting recipients in group {GroupId}", groupId);

                var count = await repo.CountGroupRecipientsAsync(groupId);
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error counting group recipients.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
