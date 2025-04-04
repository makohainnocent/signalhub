using Application.Recipients.Abstractions;
using Domain.Common.Responses;
using Domain.RecipientGroups;
using Domain.Recipients;
using Domain.Recipients.Requests;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Api.Recipients.Controllers
{
   
    public static class RecipientsController
    {
        // Create a new recipient
        public static async Task<IResult> CreateRecipientAsync(
            IRecipientsRepository repo,
            [FromBody] RecipientCreationRequest request)
        {
            try
            {
                Log.Information("Creating a new recipient with email {Email}", request.Email);

                var recipient = await repo.CreateRecipientAsync(request);
                return Results.Created($"/api/Recipients/{recipient.UserId}", recipient);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating recipient.");
                return Results.Problem(ex.Message);
            }
        }

        // Get recipients with pagination
        public static async Task<IResult> GetRecipientsAsync(
            IRecipientsRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            int? tenantId = null,
            int? userId = null,
            bool? isActive = null)
        {
            try
            {
                Log.Information("Fetching recipients with pagination: Page {PageNumber}, PageSize {PageSize}", pageNumber, pageSize);

                var recipients = await repo.GetRecipientsAsync(pageNumber, pageSize, search, tenantId, userId, isActive);
                return Results.Ok(recipients);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching recipients.");
                return Results.Problem(ex.Message);
            }
        }

        // Get recipient by ID
        public static async Task<IResult> GetRecipientByIdAsync(
            IRecipientsRepository repo,
            int recipientId)
        {
            try
            {
                Log.Information("Fetching recipient with ID {RecipientId}", recipientId);

                var recipient = await repo.GetRecipientByIdAsync(recipientId);
                return recipient != null ? Results.Ok(recipient) : Results.NotFound(new { message = "Recipient not found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching recipient.");
                return Results.Problem(ex.Message);
            }
        }

        // Update recipient
        public static async Task<IResult> UpdateRecipientAsync(
            IRecipientsRepository repo,
            [FromBody] RecipientUpdateRequest request)
        {
            try
            {
                Log.Information("Updating recipient with ID {RecipientId}", request.RecipientId);

                var updatedRecipient = await repo.UpdateRecipientAsync(request);
                return Results.Ok(updatedRecipient);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating recipient.");
                return Results.Problem(ex.Message);
            }
        }

        // Delete recipient
        public static async Task<IResult> DeleteRecipientAsync(
            IRecipientsRepository repo,
            int recipientId)
        {
            try
            {
                Log.Information("Deleting recipient with ID {RecipientId}", recipientId);

                var success = await repo.DeleteRecipientAsync(recipientId);
                return success ? Results.Ok(new { message = "Recipient deleted successfully." }) : Results.BadRequest(new { message = "Failed to delete recipient." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting recipient.");
                return Results.Problem(ex.Message);
            }
        }

        // Count recipients
        public static async Task<IResult> CountRecipientsAsync(IRecipientsRepository repo)
        {
            try
            {
                Log.Information("Counting recipients");

                var count = await repo.CountRecipientsAsync();
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error counting recipients.");
                return Results.Problem(ex.Message);
            }
        }

        // Get recipient by email
        public static async Task<IResult> GetRecipientByEmailAsync(
            IRecipientsRepository repo,
            string email,
            int tenantId)
        {
            try
            {
                Log.Information("Fetching recipient with email {Email}", email);

                var recipient = await repo.GetRecipientByEmailAsync(email, tenantId);
                return recipient != null ? Results.Ok(recipient) : Results.NotFound(new { message = "Recipient not found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching recipient by email.");
                return Results.Problem(ex.Message);
            }
        }

        // Get recipient by phone number
        public static async Task<IResult> GetRecipientByPhoneAsync(
            IRecipientsRepository repo,
            string phoneNumber,
            int tenantId)
        {
            try
            {
                Log.Information("Fetching recipient with phone number {PhoneNumber}", phoneNumber);

                var recipient = await repo.GetRecipientByPhoneAsync(phoneNumber, tenantId);
                return recipient != null ? Results.Ok(recipient) : Results.NotFound(new { message = "Recipient not found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching recipient by phone.");
                return Results.Problem(ex.Message);
            }
        }

        // Activate recipient
        public static async Task<IResult> ActivateRecipientAsync(
            IRecipientsRepository repo,
            int recipientId)
        {
            try
            {
                Log.Information("Activating recipient with ID {RecipientId}", recipientId);

                var success = await repo.ActivateRecipientAsync(recipientId);
                return success ? Results.Ok(new { message = "Recipient activated successfully." }) : Results.BadRequest(new { message = "Failed to activate recipient." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error activating recipient.");
                return Results.Problem(ex.Message);
            }
        }

        // Deactivate recipient
        public static async Task<IResult> DeactivateRecipientAsync(
            IRecipientsRepository repo,
            int recipientId)
        {
            try
            {
                Log.Information("Deactivating recipient with ID {RecipientId}", recipientId);

                var success = await repo.DeactivateRecipientAsync(recipientId);
                return success ? Results.Ok(new { message = "Recipient deactivated successfully." }) : Results.BadRequest(new { message = "Failed to deactivate recipient." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deactivating recipient.");
                return Results.Problem(ex.Message);
            }
        }

        // Update recipient preferences
        public static async Task<IResult> UpdatePreferencesAsync(
            IRecipientsRepository repo,
            int recipientId,
            [FromBody] string preferencesJson)
        {
            try
            {
                Log.Information("Updating preferences for recipient with ID {RecipientId}", recipientId);

                var success = await repo.UpdatePreferencesAsync(recipientId, preferencesJson);
                return success ? Results.Ok(new { message = "Preferences updated successfully." }) : Results.BadRequest(new { message = "Failed to update preferences." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating recipient preferences.");
                return Results.Problem(ex.Message);
            }
        }

        // Get groups for a recipient
        public static async Task<IResult> GetRecipientGroupsAsync(
            IRecipientsRepository repo,
            int recipientId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                Log.Information("Fetching groups for recipient with ID {RecipientId}", recipientId);

                var groups = await repo.GetRecipientGroupsAsync(recipientId, pageNumber, pageSize);
                return Results.Ok(groups);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching recipient groups.");
                return Results.Problem(ex.Message);
            }
        }

        // Count groups for a recipient
        public static async Task<IResult> CountRecipientGroupsAsync(
            IRecipientsRepository repo,
            int recipientId)
        {
            try
            {
                Log.Information("Counting groups for recipient with ID {RecipientId}", recipientId);

                var count = await repo.CountRecipientGroupsAsync(recipientId);
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error counting recipient groups.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
