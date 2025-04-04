using Api.Common.Abstractions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Application.RecipientGroups.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Api.RecipientGroupMembers.Controllers;

namespace Api.RecipientGroupMembers.EndPointDefinitions
{
    public class RecipientGroupMembersEndpoints : IEndpointDefinition
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

            var recipientGroupMembers = versionedGroup.MapGroup("/recipientgroupmembers")
                .WithTags("Recipient Group Members Management");

            // Add recipient to group
            recipientGroupMembers.MapPost("/groups/{groupId:int}/recipients/{recipientId:int}", async (
                IRecipientGroupMembersRepository repo,
                int groupId,
                int recipientId,
                HttpContext httpContext) =>
            {
                return await RecipientGroupMemberController.AddRecipientToGroupAsync(repo, groupId, recipientId, httpContext);
            })
            .RequireAuthorization();

            // Remove recipient from group
            recipientGroupMembers.MapDelete("/groups/{groupId:int}/recipients/{recipientId:int}", async (
                IRecipientGroupMembersRepository repo,
                int groupId,
                int recipientId) =>
            {
                return await RecipientGroupMemberController.RemoveRecipientFromGroupAsync(repo, groupId, recipientId);
            });

            // Check if recipient is in group
            recipientGroupMembers.MapGet("/groups/{groupId:int}/recipients/{recipientId:int}/membership", async (
                IRecipientGroupMembersRepository repo,
                int groupId,
                int recipientId) =>
            {
                return await RecipientGroupMemberController.IsRecipientInGroupAsync(repo, groupId, recipientId);
            });

            // Add multiple recipients to group
            recipientGroupMembers.MapPost("/groups/{groupId:int}/recipients/bulk", async (
                IRecipientGroupMembersRepository repo,
                int groupId,
                [FromBody] IEnumerable<int> recipientIds,
                HttpContext httpContext) =>
            {
                return await RecipientGroupMemberController.AddMultipleRecipientsToGroupAsync(repo, groupId, recipientIds, httpContext);
            })
            .RequireAuthorization();

            // Remove multiple recipients from group
            recipientGroupMembers.MapDelete("/groups/{groupId:int}/recipients/bulk", async (
                IRecipientGroupMembersRepository repo,
                int groupId,
                [FromBody] IEnumerable<int> recipientIds) =>
            {
                return await RecipientGroupMemberController.RemoveMultipleRecipientsFromGroupAsync(repo, groupId, recipientIds);
            });

            // Get group members with pagination
            recipientGroupMembers.MapGet("/groups/{groupId:int}/members", async (
                IRecipientGroupMembersRepository repo,
                int groupId,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] bool? isActive = null) =>
            {
                return await RecipientGroupMemberController.GetGroupMembersAsync(repo, groupId, pageNumber, pageSize, search, isActive);
            });

            // Get recipient groups with pagination
            recipientGroupMembers.MapGet("/recipients/{recipientId:int}/groups", async (
                IRecipientGroupMembersRepository repo,
                int recipientId,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                return await RecipientGroupMemberController.GetRecipientGroupsAsync(repo, recipientId, pageNumber, pageSize);
            });

            // Count group members
            recipientGroupMembers.MapGet("/groups/{groupId:int}/members/count", async (
                IRecipientGroupMembersRepository repo,
                int groupId) =>
            {
                return await RecipientGroupMemberController.CountGroupMembersAsync(repo, groupId);
            });

            // Count recipient groups
            recipientGroupMembers.MapGet("/recipients/{recipientId:int}/groups/count", async (
                IRecipientGroupMembersRepository repo,
                int recipientId) =>
            {
                return await RecipientGroupMemberController.CountRecipientGroupsAsync(repo, recipientId);
            });

            // Get membership history
            recipientGroupMembers.MapGet("/groups/{groupId:int}/recipients/{recipientId:int}/history", async (
                IRecipientGroupMembersRepository repo,
                int groupId,
                int recipientId,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                return await RecipientGroupMemberController.GetMembershipHistoryAsync(repo, groupId, recipientId, pageNumber, pageSize);
            });
        }
    }
}