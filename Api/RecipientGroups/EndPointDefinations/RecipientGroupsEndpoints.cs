using Api.Common.Abstractions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Application.RecipientGroups.Abstractions;
using Domain.RecipientGroups.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Api.RecipientGroups.Controllers;

namespace Api.RecipientGroups.EndPointDefinitions
{
    public class RecipientGroupsEndpoints : IEndpointDefinition
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

            var recipientGroups = versionedGroup.MapGroup("/recipientgroups")
                .WithTags("Recipient Groups Management");

            // Create a new recipient group
            recipientGroups.MapPost("/", async (
                IRecipientGroupsRepository repo,
                [FromBody] RecipientGroupCreationRequest request) =>
            {
                return await RecipientGroupsController.CreateRecipientGroupAsync(repo, request);
            });

            // Get recipient groups with pagination
            recipientGroups.MapGet("/", async (
                IRecipientGroupsRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] int? tenantId = null,
                [FromQuery] int? createdByUserId = null) =>
            {
                return await RecipientGroupsController.GetRecipientGroupsAsync(
                    repo, pageNumber, pageSize, search, tenantId, createdByUserId);
            });

            // Get recipient group by ID
            recipientGroups.MapGet("/{groupId:int}", async (
                IRecipientGroupsRepository repo,
                int groupId) =>
            {
                return await RecipientGroupsController.GetRecipientGroupByIdAsync(repo, groupId);
            });

            // Update recipient group
            recipientGroups.MapPut("/", async (
                IRecipientGroupsRepository repo,
                [FromBody] RecipientGroupUpdateRequest request) =>
            {
                return await RecipientGroupsController.UpdateRecipientGroupAsync(repo, request);
            });

            // Delete recipient group
            recipientGroups.MapDelete("/{groupId:int}", async (
                IRecipientGroupsRepository repo,
                int groupId) =>
            {
                return await RecipientGroupsController.DeleteRecipientGroupAsync(repo, groupId);
            });

            // Count recipient groups
            recipientGroups.MapGet("/count", async (
                IRecipientGroupsRepository repo) =>
            {
                return await RecipientGroupsController.CountRecipientGroupsAsync(repo);
            });

            // Add recipient to group
            recipientGroups.MapPost("/{groupId:int}/recipients/{recipientId:int}", async (
                IRecipientGroupsRepository repo,
                int groupId,
                int recipientId,
                HttpContext httpContext) =>
            {
                return await RecipientGroupsController.AddRecipientToGroupAsync(repo, groupId, recipientId, httpContext);
            })
            .RequireAuthorization();

            // Remove recipient from group
            recipientGroups.MapDelete("/{groupId:int}/recipients/{recipientId:int}", async (
                IRecipientGroupsRepository repo,
                int groupId,
                int recipientId) =>
            {
                return await RecipientGroupsController.RemoveRecipientFromGroupAsync(repo, groupId, recipientId);
            });

            // Get group recipients
            recipientGroups.MapGet("/{groupId:int}/recipients", async (
                IRecipientGroupsRepository repo,
                int groupId,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null) =>
            {
                return await RecipientGroupsController.GetGroupRecipientsAsync(repo, groupId, pageNumber, pageSize, search);
            });

            // Count group recipients
            recipientGroups.MapGet("/{groupId:int}/recipients/count", async (
                IRecipientGroupsRepository repo,
                int groupId) =>
            {
                return await RecipientGroupsController.CountGroupRecipientsAsync(repo, groupId);
            });
        }
    }
}