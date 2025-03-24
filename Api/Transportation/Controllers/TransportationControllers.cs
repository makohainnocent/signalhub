// Api/Transportation/Controllers/TransportationControllers.cs
using Domain.Transportation.Models;
using Domain.Transportation.Requests;
using Application.Transportations.Abstractions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Claims;
using DataAccess.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Transportation.Controllers
{
    public static class TransportationControllers
    {
        public static async Task<IResult> CreateTransportationAsync(
            ITransportationRepository repo,
            [FromBody] TransportationCreationRequest request,
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

                Log.Information("Attempting to create transportation for permit ID: {PermitId}", request.PermitId);

                // Create the transportation
                var createdTransportation = await repo.CreateTransportationAsync(request);

                Log.Information("Transportation created successfully with ID: {TransportId}", createdTransportation.TransportId);
                return Results.Created($"/transportations/{createdTransportation.TransportId}", createdTransportation);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the transportation.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetTransportationsAsync(
            ITransportationRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            int? userId = null, 
            int? frompremiseId = null,
            string? agent="no",
            string? vet = "no")
        {
            try
            {
                Log.Information("Attempting to retrieve transportations with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                // Retrieve paginated transportations
                var pagedResult = await repo.GetTransportationsAsync(pageNumber, pageSize, search,userId,frompremiseId,agent,vet);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No transportations found." });
                }

                Log.Information("Successfully retrieved {TransportationCount} transportations out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving transportations.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetTransportationByIdAsync(
            ITransportationRepository repo,
            int transportId)
        {
            try
            {
                Log.Information("Attempting to retrieve transportation with ID: {TransportId}", transportId);

                // Retrieve the transportation by ID
                var transportation = await repo.GetTransportationByIdAsync(transportId);

                if (transportation == null)
                {
                    Log.Warning("Transportation with ID: {TransportId} not found.", transportId);
                    return Results.NotFound(new { message = "Transportation not found." });
                }

                Log.Information("Successfully retrieved transportation with ID: {TransportId}", transportId);
                return Results.Ok(transportation);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the transportation.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateTransportationAsync(
            ITransportationRepository repo,
            [FromBody] TransportationUpdateRequest request)
        {
            try
            {
                Log.Information("Attempting to update transportation with ID: {TransportId}", request.TransportId);

                // Update the transportation
                var updatedTransportation = await repo.UpdateTransportationAsync(request);

                Log.Information("Transportation updated successfully with ID: {TransportId}", updatedTransportation.TransportId);
                return Results.Ok(updatedTransportation);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Transportation update failed - transportation does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the transportation.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteTransportationAsync(
            ITransportationRepository repo,
            int transportId)
        {
            try
            {
                Log.Information("Attempting to delete transportation with ID: {TransportId}", transportId);

                // Delete the transportation
                var deleted = await repo.DeleteTransportationAsync(transportId);

                if (deleted)
                {
                    Log.Information("Transportation deleted successfully with ID: {TransportId}", transportId);
                    return Results.Ok(new { message = "Transportation deleted successfully." });
                }
                else
                {
                    Log.Warning("Transportation deletion failed - transportation does not exist.");
                    return Results.NotFound(new { message = "Transportation not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the transportation.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountTransportationsAsync(
            ITransportationRepository repo)
        {
            try
            {
                Log.Information("Attempting to count all transportations.");

                // Count the transportations
                var count = await repo.CountTransportationsAsync();

                Log.Information("Successfully counted {TransportationCount} transportations.", count);
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting transportations.");
                return Results.Problem(ex.Message);
            }
        }
    }
}