using Domain.NotificationTemplates.Requests;
using Application.NotificationTemplates.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Domain.NotificationTemplates;
using Domain.TemplateChannels;
using Domain.Common.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Claims;

namespace Api.NotificationTemplates.Controllers
{
    public static class NotificationTemplatesController
    {
        // Create a new Notification Template
        public static async Task<IResult> CreateTemplateAsync(
            INotificationTemplatesRepository repo,
            [FromBody] NotificationTemplateCreationRequest request,
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

                Log.Information("Creating new notification template for user ID: {UserId}", userId);

                var template = await repo.CreateTemplateAsync(request);

                Log.Information("Successfully created template with ID: {TemplateId}", template.TemplateId);
                return Results.Created($"/notificationtemplates/{template.TemplateId}", template);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the notification template.");
                return Results.Problem(ex.Message);
            }
        }

        // Get all templates with pagination
        public static async Task<IResult> GetTemplatesAsync(
            INotificationTemplatesRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            int? applicationId = null,
            int? createdByUserId = null,
            string? approvalStatus = null,
            bool? isActive = null)
        {
            try
            {
                Log.Information("Retrieving templates with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                var pagedResult = await repo.GetTemplatesAsync(pageNumber, pageSize, search, applicationId, createdByUserId, approvalStatus, isActive);

                if (pagedResult.Items == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No templates found." });
                }

                Log.Information("Successfully retrieved {TemplateCount} templates out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving templates.");
                return Results.Problem(ex.Message);
            }
        }

        // Get a template by ID
        public static async Task<IResult> GetTemplateByIdAsync(
            INotificationTemplatesRepository repo,
            int templateId)
        {
            try
            {
                Log.Information("Attempting to retrieve template with ID: {TemplateId}", templateId);

                var template = await repo.GetTemplateByIdAsync(templateId);

                if (template == null)
                {
                    Log.Warning("Template with ID: {TemplateId} not found.", templateId);
                    return Results.NotFound(new { message = "Template not found." });
                }

                Log.Information("Successfully retrieved template with ID: {TemplateId}", templateId);
                return Results.Ok(template);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the template.");
                return Results.Problem(ex.Message);
            }
        }

        // Update a template
        public static async Task<IResult> UpdateTemplateAsync(
            INotificationTemplatesRepository repo,
            [FromBody] NotificationTemplateUpdateRequest request,
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

                Log.Information("Attempting to update template with ID: {TemplateId} for user ID: {UserId}", request.TemplateId, userId);

                var updatedTemplate = await repo.UpdateTemplateAsync(request);

                Log.Information("Successfully updated template with ID: {TemplateId}", request.TemplateId);
                return Results.Ok(updatedTemplate);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the template.");
                return Results.Problem(ex.Message);
            }
        }

        // Delete a template
        public static async Task<IResult> DeleteTemplateAsync(
            INotificationTemplatesRepository repo,
            int templateId)
        {
            try
            {
                Log.Information("Attempting to delete template with ID: {TemplateId}", templateId);

                var result = await repo.DeleteTemplateAsync(templateId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to delete the template." });
                }

                Log.Information("Successfully deleted template with ID: {TemplateId}", templateId);
                return Results.Ok(new { message = "Template deleted successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the template.");
                return Results.Problem(ex.Message);
            }
        }

        // Activate a template
        public static async Task<IResult> ActivateTemplateAsync(
            INotificationTemplatesRepository repo,
            int templateId)
        {
            try
            {
                Log.Information("Attempting to activate template with ID: {TemplateId}", templateId);

                var result = await repo.ActivateTemplateAsync(templateId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to activate the template." });
                }

                Log.Information("Successfully activated template with ID: {TemplateId}", templateId);
                return Results.Ok(new { message = "Template activated successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while activating the template.");
                return Results.Problem(ex.Message);
            }
        }

        // Deactivate a template
        public static async Task<IResult> DeactivateTemplateAsync(
            INotificationTemplatesRepository repo,
            int templateId)
        {
            try
            {
                Log.Information("Attempting to deactivate template with ID: {TemplateId}", templateId);

                var result = await repo.DeactivateTemplateAsync(templateId);

                if (!result)
                {
                    return Results.BadRequest(new { message = "Failed to deactivate the template." });
                }

                Log.Information("Successfully deactivated template with ID: {TemplateId}", templateId);
                return Results.Ok(new { message = "Template deactivated successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deactivating the template.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
