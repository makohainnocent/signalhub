
using Domain.PremiseOwner.PremiseOwnerCreateRequest;
using Domain.PremiseOwner.Requests;
using Serilog;
using System.Security.Claims;
using DataAccess.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Application.PremiseOwners.Abstraction;

namespace Api.PremiseOwners.Controllers
{
    public static class PremiseOwnerController
    {
        // Create a new premise owner
        public static async Task<IResult> CreatePremiseOwnerAsync(
            IPremiseOwnerRepository repo,
            [FromBody] PremiseOwnerCreateRequest request,
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

                Log.Information("Attempting to create premise owner for: {OwnerName}", request.Names);

                // Create the premise owner
                var createdPremiseOwner = await repo.CreatePremiseOwnerAsync(request);

                Log.Information("Premise owner created successfully with ID: {PremiseOwnerId}", createdPremiseOwner.Id);
                return Results.Created($"/premiseowners/{createdPremiseOwner.Id}", createdPremiseOwner);
            }
            catch (ItemAlreadyExistsException ex)
            {
                Log.Warning(ex, "Premise owner creation failed - owner already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the premise owner.");
                return Results.Problem(ex.Message);
            }
        }

        // Get all premise owners with pagination
        public static async Task<IResult> GetPremiseOwnersAsync(
            IPremiseOwnerRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            int? registeredBy = 0)
        {
            try
            {
                Log.Information("Attempting to retrieve premise owners with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                // Retrieve paginated premise owners
                var pagedResult = await repo.GetPremiseOwnersAsync(pageNumber, pageSize, search,registeredBy);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No premise owners found." });
                }

                Log.Information("Successfully retrieved {PremiseOwnerCount} premise owners out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving premise owners.");
                return Results.Problem(ex.Message);
            }
        }

        // Get a premise owner by ID
        public static async Task<IResult> GetPremiseOwnerByIdAsync(
            IPremiseOwnerRepository repo,
            int premiseOwnerId)
        {
            try
            {
                Log.Information("Attempting to retrieve premise owner with ID: {PremiseOwnerId}", premiseOwnerId);

                // Retrieve the premise owner by ID
                var premiseOwner = await repo.GetPremiseOwnerByIdAsync(premiseOwnerId);

                if (premiseOwner == null)
                {
                    Log.Warning("Premise owner with ID: {PremiseOwnerId} not found.", premiseOwnerId);
                    return Results.NotFound(new { message = "Premise owner not found." });
                }

                Log.Information("Successfully retrieved premise owner with ID: {PremiseOwnerId}", premiseOwnerId);
                return Results.Ok(premiseOwner);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the premise owner.");
                return Results.Problem(ex.Message);
            }
        }

        // Update a premise owner
        public static async Task<IResult> UpdatePremiseOwnerAsync(
            IPremiseOwnerRepository repo,
            [FromBody] PremiseOwnerUpdateRequest request)
        {
            try
            {
                Log.Information("Attempting to update premise owner with ID: {PremiseOwnerId}", request.Id);

                // Update the premise owner
                var updatedPremiseOwner = await repo.UpdatePremiseOwnerAsync(request.Id, request);

                Log.Information("Premise owner updated successfully with ID: {PremiseOwnerId}", updatedPremiseOwner.Id);
                return Results.Ok(updatedPremiseOwner);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Premise owner update failed - owner does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the premise owner.");
                return Results.Problem(ex.Message);
            }
        }

        // Delete a premise owner
        public static async Task<IResult> DeletePremiseOwnerAsync(
            IPremiseOwnerRepository repo,
            int premiseOwnerId)
        {
            try
            {
                Log.Information("Attempting to delete premise owner with ID: {PremiseOwnerId}", premiseOwnerId);

                // Delete the premise owner
                var deleted = await repo.DeletePremiseOwnerAsync(premiseOwnerId);

                if (deleted)
                {
                    Log.Information("Premise owner deleted successfully with ID: {PremiseOwnerId}", premiseOwnerId);
                    return Results.Ok(new { message = "Premise owner deleted successfully." });
                }
                else
                {
                    Log.Warning("Premise owner deletion failed - owner does not exist.");
                    return Results.NotFound(new { message = "Premise owner not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the premise owner.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
