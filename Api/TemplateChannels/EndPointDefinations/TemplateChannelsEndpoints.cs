using Api.Common.Abstractions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Application.NotificationTemplates.Abstractions;
using Domain.TemplateChannels.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Api.NotificationTemplates.Controllers;

namespace Api.NotificationTemplates.EndPointDefinitions
{
    public class TemplateChannelsEndpoints : IEndpointDefinition
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

            var templateChannels = versionedGroup.MapGroup("/templatechannels")
                .WithTags("Template Channels Management");

            // Create a new template channel
            templateChannels.MapPost("/", async (
                ITemplateChannelsRepository repo,
                [FromBody] TemplateChannelCreationRequest request) =>
            {
                return await TemplateChannelsController.CreateTemplateChannelAsync(repo, request);
            });

            // Get template channel by ID
            templateChannels.MapGet("/{templateChannelId:int}", async (
                ITemplateChannelsRepository repo,
                int templateChannelId) =>
            {
                return await TemplateChannelsController.GetTemplateChannelByIdAsync(repo, templateChannelId);
            });

            // Update template channel
            templateChannels.MapPut("/", async (
                ITemplateChannelsRepository repo,
                [FromBody] TemplateChannelUpdateRequest request) =>
            {
                return await TemplateChannelsController.UpdateTemplateChannelAsync(repo, request);
            });

            // Delete template channel
            templateChannels.MapDelete("/{templateChannelId:int}", async (
                ITemplateChannelsRepository repo,
                int templateChannelId) =>
            {
                return await TemplateChannelsController.DeleteTemplateChannelAsync(repo, templateChannelId);
            });

            // Get channels by template
            templateChannels.MapGet("/templates/{templateId:int}", async (
                ITemplateChannelsRepository repo,
                int templateId,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? channelType = null,
                [FromQuery] bool? isActive = null) =>
            {
                return await TemplateChannelsController.GetChannelsByTemplateAsync(
                    repo, templateId, pageNumber, pageSize, channelType, isActive);
            });

            // Get channel by type
            templateChannels.MapGet("/templates/{templateId:int}/types/{channelType}", async (
                ITemplateChannelsRepository repo,
                int templateId,
                string channelType) =>
            {
                return await TemplateChannelsController.GetChannelByTypeAsync(repo, templateId, channelType);
            });

            // Update channel content
            templateChannels.MapPut("/{templateChannelId:int}/content", async (
                ITemplateChannelsRepository repo,
                int templateChannelId,
                [FromBody] string channelContentJson,
                [FromQuery] int updatedByUserId) =>
            {
                return await TemplateChannelsController.UpdateChannelContentAsync(
                    repo, templateChannelId, channelContentJson, updatedByUserId);
            });

            // Activate channel
            templateChannels.MapPut("/{templateChannelId:int}/activate", async (
                ITemplateChannelsRepository repo,
                int templateChannelId) =>
            {
                return await TemplateChannelsController.ActivateChannelAsync(repo, templateChannelId);
            });

            // Deactivate channel
            templateChannels.MapPut("/{templateChannelId:int}/deactivate", async (
                ITemplateChannelsRepository repo,
                int templateChannelId) =>
            {
                return await TemplateChannelsController.DeactivateChannelAsync(repo, templateChannelId);
            });

            // Check channel type existence
            templateChannels.MapGet("/templates/{templateId:int}/types/{channelType}/exists", async (
                ITemplateChannelsRepository repo,
                int templateId,
                string channelType) =>
            {
                return await TemplateChannelsController.ChannelTypeExistsForTemplateAsync(repo, templateId, channelType);
            });

            // Count channels by template
            templateChannels.MapGet("/templates/{templateId:int}/count", async (
                ITemplateChannelsRepository repo,
                int templateId) =>
            {
                return await TemplateChannelsController.CountChannelsByTemplateAsync(repo, templateId);
            });

            // Count active channels by template
            templateChannels.MapGet("/templates/{templateId:int}/active-count", async (
                ITemplateChannelsRepository repo,
                int templateId) =>
            {
                return await TemplateChannelsController.CountActiveChannelsByTemplateAsync(repo, templateId);
            });
        }
    }
}