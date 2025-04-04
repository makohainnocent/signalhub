using Application.RecipientGroups.Abstractions;
using Domain.RecipientGroups;
using Domain.Recipients;
using Domain.Common.Responses;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Api.RecipientGroupMembers.Controllers
{
    
    public static class RecipientGroupMemberController
    {
        // Add a recipient to a group
        public static async Task<IResult> AddRecipientToGroupAsync(
            IRecipientGroupMembersRepository repo,
            int groupId,
            int recipientId,
            HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to add recipient {RecipientId} to group {GroupId} by user {UserId}", recipientId, groupId, userId);

                var result = await repo.AddRecipientToGroupAsync(groupId, recipientId, userId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to add recipient to group." });
                }

                Log.Information("Successfully added recipient {RecipientId} to group {GroupId}", recipientId, groupId);
                return Results.Ok(new { message = "Recipient added successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding recipient to group.");
                return Results.Problem(ex.Message);
            }
        }

        // Remove a recipient from a group
        public static async Task<IResult> RemoveRecipientFromGroupAsync(
            IRecipientGroupMembersRepository repo,
            int groupId,
            int recipientId)
        {
            try
            {
                Log.Information("Attempting to remove recipient {RecipientId} from group {GroupId}", recipientId, groupId);

                var result = await repo.RemoveRecipientFromGroupAsync(groupId, recipientId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to remove recipient from group." });
                }

                Log.Information("Successfully removed recipient {RecipientId} from group {GroupId}", recipientId, groupId);
                return Results.Ok(new { message = "Recipient removed successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while removing recipient from group.");
                return Results.Problem(ex.Message);
            }
        }

        // Check if recipient is in group
        public static async Task<IResult> IsRecipientInGroupAsync(
            IRecipientGroupMembersRepository repo,
            int groupId,
            int recipientId)
        {
            try
            {
                Log.Information("Checking if recipient {RecipientId} is in group {GroupId}", recipientId, groupId);

                var result = await repo.IsRecipientInGroupAsync(groupId, recipientId);

                return Results.Ok(new { isInGroup = result });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while checking recipient membership.");
                return Results.Problem(ex.Message);
            }
        }

        // Add multiple recipients to a group
        public static async Task<IResult> AddMultipleRecipientsToGroupAsync(
            IRecipientGroupMembersRepository repo,
            int groupId,
            [FromBody] IEnumerable<int> recipientIds,
            HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to add multiple recipients to group {GroupId} by user {UserId}", groupId, userId);

                var addedCount = await repo.AddMultipleRecipientsToGroupAsync(groupId, recipientIds, userId);

                Log.Information("Successfully added {AddedCount} recipients to group {GroupId}", addedCount, groupId);
                return Results.Ok(new { message = $"{addedCount} recipients added successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding multiple recipients to the group.");
                return Results.Problem(ex.Message);
            }
        }

        // Remove multiple recipients from a group
        public static async Task<IResult> RemoveMultipleRecipientsFromGroupAsync(
            IRecipientGroupMembersRepository repo,
            int groupId,
            [FromBody] IEnumerable<int> recipientIds)
        {
            try
            {
                Log.Information("Attempting to remove multiple recipients from group {GroupId}", groupId);

                var removedCount = await repo.RemoveMultipleRecipientsFromGroupAsync(groupId, recipientIds);

                Log.Information("Successfully removed {RemovedCount} recipients from group {GroupId}", removedCount, groupId);
                return Results.Ok(new { message = $"{removedCount} recipients removed successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while removing multiple recipients from the group.");
                return Results.Problem(ex.Message);
            }
        }

        // Get all members of a group with pagination
        public static async Task<IResult> GetGroupMembersAsync(
            IRecipientGroupMembersRepository repo,
            int groupId,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            bool? isActive = null)
        {
            try
            {
                Log.Information("Retrieving members of group {GroupId} with pagination: Page {PageNumber}, PageSize {PageSize}.", groupId, pageNumber, pageSize);

                var pagedResult = await repo.GetGroupMembersAsync(groupId, pageNumber, pageSize, search, isActive);

                if (pagedResult.Items == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No members found in the group." });
                }

                Log.Information("Successfully retrieved {MemberCount} members from group {GroupId}.", pagedResult.Items.Count(), groupId);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving group members.");
                return Results.Problem(ex.Message);
            }
        }

        // Get recipient groups with pagination
        public static async Task<IResult> GetRecipientGroupsAsync(
            IRecipientGroupMembersRepository repo,
            int recipientId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                Log.Information("Retrieving recipient groups for recipient {RecipientId} with pagination: Page {PageNumber}, PageSize {PageSize}.", recipientId, pageNumber, pageSize);

                var pagedResult = await repo.GetRecipientGroupsAsync(recipientId, pageNumber, pageSize);

                if (pagedResult.Items == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No groups found for recipient." });
                }

                Log.Information("Successfully retrieved {GroupCount} groups for recipient {RecipientId}.", pagedResult.Items.Count(), recipientId);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving recipient groups.");
                return Results.Problem(ex.Message);
            }
        }

        // Count the number of members in a group
        public static async Task<IResult> CountGroupMembersAsync(
            IRecipientGroupMembersRepository repo,
            int groupId)
        {
            try
            {
                Log.Information("Counting the number of members in group {GroupId}", groupId);

                var count = await repo.CountGroupMembersAsync(groupId);

                return Results.Ok(new { memberCount = count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting group members.");
                return Results.Problem(ex.Message);
            }
        }

        // Count the number of groups a recipient belongs to
        public static async Task<IResult> CountRecipientGroupsAsync(
            IRecipientGroupMembersRepository repo,
            int recipientId)
        {
            try
            {
                Log.Information("Counting the number of groups recipient {RecipientId} belongs to", recipientId);

                var count = await repo.CountRecipientGroupsAsync(recipientId);

                return Results.Ok(new { groupCount = count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting recipient groups.");
                return Results.Problem(ex.Message);
            }
        }

        // Get membership history
        public static async Task<IResult> GetMembershipHistoryAsync(
            IRecipientGroupMembersRepository repo,
            int groupId,
            int recipientId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                Log.Information("Retrieving membership history for recipient {RecipientId} in group {GroupId}", recipientId, groupId);

                var pagedResult = await repo.GetMembershipHistoryAsync(groupId, recipientId, pageNumber, pageSize);

                if (pagedResult.Items == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No membership history found." });
                }

                Log.Information("Successfully retrieved membership history for recipient {RecipientId} in group {GroupId}.", recipientId, groupId);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving membership history.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
