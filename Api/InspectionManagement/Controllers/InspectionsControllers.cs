using Application.InspectionManagement.Abstractions;
using Application.LivestockManagement.Abstractions;
using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.InspectionManagement.Requests;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Api.InspectionManagement.Controllers
{
    public class InspectionsControllers
    {
        public static async Task<IResult> CreateInspectionAsync(IInspectionRepository repo, [FromBody] CreateInspectionRequest request, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create inspection by user ID: {UserId}", userId);

                request.UserId = userId;
                Inspection createdInspection = await repo.CreateInspectionAsync(request);

                Log.Information("Inspection created successfully with ID: {InspectionId} by user ID: {UserId}", createdInspection.InspectionId, userId);
                return Results.Created($"/inspections/{createdInspection.InspectionId}", createdInspection);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the inspection.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetAllInspectionsAsync(IInspectionRepository repo, int pageNumber, int pageSize, string? search = null,int? userId=0)
        {
            try
            {
                Log.Information("Attempting to retrieve inspections with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                PagedResultResponse<Inspection> pagedResult = await repo.GetAllInspectionsAsync(pageNumber, pageSize, search,userId);

                if (pagedResult.Items == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No inspections found." });
                }

                Log.Information("Successfully retrieved {InspectionCount} inspections out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving inspections.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetInspectionByIdAsync(IInspectionRepository repo, int inspectionId)
        {
            try
            {
                Log.Information("Attempting to retrieve inspection with ID: {InspectionId}", inspectionId);

                Inspection? inspection = await repo.GetInspectionByIdAsync(inspectionId);

                if (inspection == null)
                {
                    Log.Warning("Inspection with ID: {InspectionId} not found.", inspectionId);
                    return Results.NotFound(new { message = "Inspection not found." });
                }

                Log.Information("Successfully retrieved inspection with ID: {InspectionId}", inspectionId);
                return Results.Ok(inspection);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving inspection details.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateInspectionAsync(IInspectionRepository repo, int inspectionId, [FromBody] Inspection inspection)
        {
            try
            {
                Log.Information("Attempting to update inspection with ID: {InspectionId}", inspectionId);

                Inspection updatedInspection = await repo.UpdateInspectionAsync(inspection);

                Log.Information("Inspection updated successfully with ID: {InspectionId}", updatedInspection.InspectionId);
                return Results.Ok(updatedInspection);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the inspection.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteInspectionAsync(IInspectionRepository repo, int inspectionId)
        {
            try
            {
                Log.Information("Attempting to delete inspection with ID: {InspectionId}", inspectionId);

                bool deleted = await repo.DeleteInspectionAsync(inspectionId);

                if (deleted)
                {
                    Log.Information("Inspection deleted successfully with ID: {InspectionId}", inspectionId);
                    return Results.Ok("Inspection deleted successfully.");
                }
                else
                {
                    return Results.NotFound(new { message = $"Inspection with ID {inspectionId} does not exist." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the inspection.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountInspections(
            IInspectionRepository repo, int? userId = null, int? livestockId = null, int? farmId = null)


        {
            try
            {
                
                int count = await repo.CountInspectionsAsync(userId, livestockId, farmId);

                
                if (count == 0)
                {
                    return Results.NotFound(new { message = "No inspection records found for the specified criteria." });
                }

                return Results.Ok(new { count = count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting inspection records.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
