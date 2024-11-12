using Application.Approvals.Abstractions;
using Domain.Approvals.Requests;
using Domain.Common.Responses;
using Domain.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Api.Approvals.ApprovalsControllers
{
    public class ApprovalsControllers
    {
        public static async Task<IResult> CreateApprovalAsync(IApprovalsRepository repo, [FromBody] CreateApprovalRequest request, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create approval by user ID: {UserId}", userId);

                request.UserId = userId;
                Approval createdApproval = await repo.CreateApprovalAsync(request);

                Log.Information("Approval created successfully with ID: {ApprovalId} by user ID: {UserId}", createdApproval.ApprovalId, userId);
                return Results.Created($"/approvals/{createdApproval.ApprovalId}", createdApproval);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the approval.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetAllApprovalsAsync(IApprovalsRepository repo, int pageNumber, int pageSize, string? search = null)
        {
            try
            {
                Log.Information("Attempting to retrieve approvals with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                PagedResultResponse<Approval> pagedResult = await repo.GetAllApprovalsAsync(pageNumber, pageSize, search);

                if (pagedResult.Items == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No approvals found." });
                }

                Log.Information("Successfully retrieved {ApprovalCount} approvals out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving approvals.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetApprovalByIdAsync(IApprovalsRepository repo, int approvalId)
        {
            try
            {
                Log.Information("Attempting to retrieve approval with ID: {ApprovalId}", approvalId);

                Approval? approval = await repo.GetApprovalByIdAsync(approvalId);

                if (approval == null)
                {
                    Log.Warning("Approval with ID: {ApprovalId} not found.", approvalId);
                    return Results.NotFound(new { message = "Approval not found." });
                }

                Log.Information("Successfully retrieved approval with ID: {ApprovalId}", approvalId);
                return Results.Ok(approval);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving approval details.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateApprovalAsync(IApprovalsRepository repo, int approvalId, [FromBody] Approval approval)
        {
            try
            {
                Log.Information("Attempting to update approval with ID: {ApprovalId}", approvalId);

                Approval updatedApproval = await repo.UpdateApprovalAsync(approval);

                Log.Information("Approval updated successfully with ID: {ApprovalId}", updatedApproval.ApprovalId);
                return Results.Ok(updatedApproval);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the approval.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteApprovalAsync(IApprovalsRepository repo, int approvalId)
        {
            try
            {
                Log.Information("Attempting to delete approval with ID: {ApprovalId}", approvalId);

                bool deleted = await repo.DeleteApprovalAsync(approvalId);

                if (deleted)
                {
                    Log.Information("Approval deleted successfully with ID: {ApprovalId}", approvalId);
                    return Results.Ok("Approval deleted successfully.");
                }
                else
                {
                    return Results.NotFound(new { message = $"Approval with ID {approvalId} does not exist." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the approval.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
