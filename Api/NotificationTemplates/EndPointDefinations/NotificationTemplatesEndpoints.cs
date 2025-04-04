using Api.Common.Abstractions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Application.NotificationTemplates.Abstractions;
using Domain.NotificationTemplates.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Api.NotificationTemplates.Controllers;

namespace Api.NotificationTemplates.EndPointDefinitions
{
    public class NotificationTemplatesEndpoints : IEndpointDefinition
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

            var notificationTemplates = versionedGroup.MapGroup("/notificationtemplates")
                .WithTags("Notification Templates Management");

            // Create a new template
            notificationTemplates.MapPost("/", async (
                INotificationTemplatesRepository repo,
                [FromBody] NotificationTemplateCreationRequest request,
                HttpContext httpContext) =>
            {
                return await NotificationTemplatesController.CreateTemplateAsync(repo, request, httpContext);
            })
            .RequireAuthorization();

            // Get all templates (paginated with filters)
            notificationTemplates.MapGet("/", async (
                INotificationTemplatesRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] int? applicationId = null,
                [FromQuery] int? createdByUserId = null,
                [FromQuery] string? approvalStatus = null,
                [FromQuery] bool? isActive = null) =>
            {
                return await NotificationTemplatesController.GetTemplatesAsync(
                    repo, pageNumber, pageSize, search, applicationId,
                    createdByUserId, approvalStatus, isActive);
            });

            // Get template by ID
            notificationTemplates.MapGet("/{templateId:int}", async (
                INotificationTemplatesRepository repo,
                int templateId) =>
            {
                return await NotificationTemplatesController.GetTemplateByIdAsync(repo, templateId);
            });

            // Update template
            notificationTemplates.MapPut("/", async (
                INotificationTemplatesRepository repo,
                [FromBody] NotificationTemplateUpdateRequest request,
                HttpContext httpContext) =>
            {
                return await NotificationTemplatesController.UpdateTemplateAsync(repo, request, httpContext);
            })
            .RequireAuthorization();

            // Delete template
            notificationTemplates.MapDelete("/{templateId:int}", async (
                INotificationTemplatesRepository repo,
                int templateId) =>
            {
                return await NotificationTemplatesController.DeleteTemplateAsync(repo, templateId);
            });

            // Activate template
            notificationTemplates.MapPut("/{templateId:int}/activate", async (
                INotificationTemplatesRepository repo,
                int templateId) =>
            {
                return await NotificationTemplatesController.ActivateTemplateAsync(repo, templateId);
            });

            // Deactivate template
            notificationTemplates.MapPut("/{templateId:int}/deactivate", async (
                INotificationTemplatesRepository repo,
                int templateId) =>
            {
                return await NotificationTemplatesController.DeactivateTemplateAsync(repo, templateId);
            });
        }
    }
}