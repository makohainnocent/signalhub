using Api.Common.Abstractions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Application.Recipients.Abstractions;
using Domain.Recipients.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Api.Recipients.Controllers;

namespace Api.Recipients.EndPointDefinitions
{
    public class RecipientsEndpoints : IEndpointDefinition
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

            var recipients = versionedGroup.MapGroup("/recipients")
                .WithTags("Recipients Management");

            // Create a new recipient
            recipients.MapPost("/", async (
                IRecipientsRepository repo,
                [FromBody] RecipientCreationRequest request) =>
            {
                return await RecipientsController.CreateRecipientAsync(repo, request);
            });

            // Get recipients with pagination
            recipients.MapGet("/", async (
                IRecipientsRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] int? tenantId = null,
                [FromQuery] int? userId = null,
                [FromQuery] bool? isActive = null) =>
            {
                return await RecipientsController.GetRecipientsAsync(
                    repo, pageNumber, pageSize, search, tenantId, userId, isActive);
            });

            // Get recipient by ID
            recipients.MapGet("/{recipientId:int}", async (
                IRecipientsRepository repo,
                int recipientId) =>
            {
                return await RecipientsController.GetRecipientByIdAsync(repo, recipientId);
            });

            // Update recipient
            recipients.MapPut("/", async (
                IRecipientsRepository repo,
                [FromBody] RecipientUpdateRequest request) =>
            {
                return await RecipientsController.UpdateRecipientAsync(repo, request);
            });

            // Delete recipient
            recipients.MapDelete("/{recipientId:int}", async (
                IRecipientsRepository repo,
                int recipientId) =>
            {
                return await RecipientsController.DeleteRecipientAsync(repo, recipientId);
            });

            // Count recipients
            recipients.MapGet("/count", async (
                IRecipientsRepository repo) =>
            {
                return await RecipientsController.CountRecipientsAsync(repo);
            });

            // Get recipient by email
            recipients.MapGet("/by-email/{email}", async (
                IRecipientsRepository repo,
                string email,
                [FromQuery] int tenantId) =>
            {
                return await RecipientsController.GetRecipientByEmailAsync(repo, email, tenantId);
            });

            // Get recipient by phone
            recipients.MapGet("/by-phone/{phoneNumber}", async (
                IRecipientsRepository repo,
                string phoneNumber,
                [FromQuery] int tenantId) =>
            {
                return await RecipientsController.GetRecipientByPhoneAsync(repo, phoneNumber, tenantId);
            });

            // Activate recipient
            recipients.MapPut("/{recipientId:int}/activate", async (
                IRecipientsRepository repo,
                int recipientId) =>
            {
                return await RecipientsController.ActivateRecipientAsync(repo, recipientId);
            });

            // Deactivate recipient
            recipients.MapPut("/{recipientId:int}/deactivate", async (
                IRecipientsRepository repo,
                int recipientId) =>
            {
                return await RecipientsController.DeactivateRecipientAsync(repo, recipientId);
            });

            // Update recipient preferences
            recipients.MapPut("/{recipientId:int}/preferences", async (
                IRecipientsRepository repo,
                int recipientId,
                [FromBody] string preferencesJson) =>
            {
                return await RecipientsController.UpdatePreferencesAsync(repo, recipientId, preferencesJson);
            });

            // Get recipient groups
            recipients.MapGet("/{recipientId:int}/groups", async (
                IRecipientsRepository repo,
                int recipientId,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                return await RecipientsController.GetRecipientGroupsAsync(repo, recipientId, pageNumber, pageSize);
            });

            // Count recipient groups
            recipients.MapGet("/{recipientId:int}/groups/count", async (
                IRecipientsRepository repo,
                int recipientId) =>
            {
                return await RecipientsController.CountRecipientGroupsAsync(repo, recipientId);
            });
        }
    }
}