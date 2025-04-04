using Application.NotificationTemplates.Abstractions;
using Domain.Common.Responses;
using Domain.TemplateChannels;
using Domain.TemplateChannels.Requests;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Api.NotificationTemplates.Controllers
{

    [ApiController]
    public static class TemplateChannelsController
    {
        // Create a new template channel
        public static async Task<IResult> CreateTemplateChannelAsync(
            ITemplateChannelsRepository repo,
            [FromBody] TemplateChannelCreationRequest request)
        {
            try
            {
                Log.Information("Creating a new template channel for Template ID {TemplateId}", request.TemplateId);

                var channel = await repo.CreateTemplateChannelAsync(request);
                return Results.Created($"/api/TemplateChannels/{channel.TemplateId}", channel);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating template channel.");
                return Results.Problem(ex.Message);
            }
        }

        // Get template channel by ID
        public static async Task<IResult> GetTemplateChannelByIdAsync(
            ITemplateChannelsRepository repo,
            int templateChannelId)
        {
            try
            {
                Log.Information("Fetching template channel with ID {TemplateChannelId}", templateChannelId);

                var channel = await repo.GetTemplateChannelByIdAsync(templateChannelId);
                return channel != null ? Results.Ok(channel) : Results.NotFound(new { message = "Template channel not found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching template channel.");
                return Results.Problem(ex.Message);
            }
        }

        // Update template channel
        public static async Task<IResult> UpdateTemplateChannelAsync(
            ITemplateChannelsRepository repo,
            [FromBody] TemplateChannelUpdateRequest request)
        {
            try
            {
                Log.Information("Updating template channel with ID {TemplateChannelId}", request.TemplateChannelId);

                var updatedChannel = await repo.UpdateTemplateChannelAsync(request);
                return Results.Ok(updatedChannel);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating template channel.");
                return Results.Problem(ex.Message);
            }
        }

        // Delete template channel
        public static async Task<IResult> DeleteTemplateChannelAsync(
            ITemplateChannelsRepository repo,
            int templateChannelId)
        {
            try
            {
                Log.Information("Deleting template channel with ID {TemplateChannelId}", templateChannelId);

                var success = await repo.DeleteTemplateChannelAsync(templateChannelId);
                return success ? Results.Ok(new { message = "Template channel deleted successfully." }) : Results.BadRequest(new { message = "Failed to delete template channel." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting template channel.");
                return Results.Problem(ex.Message);
            }
        }

        // Get template channels by template
        public static async Task<IResult> GetChannelsByTemplateAsync(
            ITemplateChannelsRepository repo,
            int templateId,
            int pageNumber = 1,
            int pageSize = 10,
            string? channelType = null,
            bool? isActive = null)
        {
            try
            {
                Log.Information("Fetching channels for Template ID {TemplateId}", templateId);

                var channels = await repo.GetChannelsByTemplateAsync(templateId, pageNumber, pageSize, channelType, isActive);
                return Results.Ok(channels);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching template channels.");
                return Results.Problem(ex.Message);
            }
        }

        // Get channel by type
        public static async Task<IResult> GetChannelByTypeAsync(
            ITemplateChannelsRepository repo,
            int templateId,
            string channelType)
        {
            try
            {
                Log.Information("Fetching channel of type {ChannelType} for Template ID {TemplateId}", channelType, templateId);

                var channel = await repo.GetChannelByTypeAsync(templateId, channelType);
                return channel != null ? Results.Ok(channel) : Results.NotFound(new { message = "Channel not found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching channel by type.");
                return Results.Problem(ex.Message);
            }
        }

        // Update channel content
        public static async Task<IResult> UpdateChannelContentAsync(
            ITemplateChannelsRepository repo,
            int templateChannelId,
            [FromBody] string channelContentJson,
            int updatedByUserId)
        {
            try
            {
                Log.Information("Updating content for template channel ID {TemplateChannelId}", templateChannelId);

                var success = await repo.UpdateChannelContentAsync(templateChannelId, channelContentJson, updatedByUserId);
                return success ? Results.Ok(new { message = "Content updated successfully." }) : Results.BadRequest(new { message = "Failed to update content." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating channel content.");
                return Results.Problem(ex.Message);
            }
        }

        // Activate channel
        public static async Task<IResult> ActivateChannelAsync(
            ITemplateChannelsRepository repo,
            int templateChannelId)
        {
            try
            {
                Log.Information("Activating template channel with ID {TemplateChannelId}", templateChannelId);

                var success = await repo.ActivateChannelAsync(templateChannelId);
                return success ? Results.Ok(new { message = "Channel activated successfully." }) : Results.BadRequest(new { message = "Failed to activate channel." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error activating channel.");
                return Results.Problem(ex.Message);
            }
        }

        // Deactivate channel
        public static async Task<IResult> DeactivateChannelAsync(
            ITemplateChannelsRepository repo,
            int templateChannelId)
        {
            try
            {
                Log.Information("Deactivating template channel with ID {TemplateChannelId}", templateChannelId);

                var success = await repo.DeactivateChannelAsync(templateChannelId);
                return success ? Results.Ok(new { message = "Channel deactivated successfully." }) : Results.BadRequest(new { message = "Failed to deactivate channel." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deactivating channel.");
                return Results.Problem(ex.Message);
            }
        }

        // Check if a channel type exists for a template
        public static async Task<IResult> ChannelTypeExistsForTemplateAsync(
            ITemplateChannelsRepository repo,
            int templateId,
            string channelType)
        {
            try
            {
                Log.Information("Checking if channel type {ChannelType} exists for Template ID {TemplateId}", channelType, templateId);

                var exists = await repo.ChannelTypeExistsForTemplateAsync(templateId, channelType);
                return Results.Ok(new { exists });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking channel type existence.");
                return Results.Problem(ex.Message);
            }
        }

        // Count channels for a template
        public static async Task<IResult> CountChannelsByTemplateAsync(
            ITemplateChannelsRepository repo,
            int templateId)
        {
            try
            {
                Log.Information("Counting channels for Template ID {TemplateId}", templateId);

                var count = await repo.CountChannelsByTemplateAsync(templateId);
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error counting channels.");
                return Results.Problem(ex.Message);
            }
        }

        // Count active channels for a template
        public static async Task<IResult> CountActiveChannelsByTemplateAsync(
            ITemplateChannelsRepository repo,
            int templateId)
        {
            try
            {
                Log.Information("Counting active channels for Template ID {TemplateId}", templateId);

                var count = await repo.CountActiveChannelsByTemplateAsync(templateId);
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error counting active channels.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
