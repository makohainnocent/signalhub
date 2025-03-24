
using Domain.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using DataAccess.Common.Exceptions;


using Application.PremiseManagement.Abstractions;
using Domain.PremiseManagement.Requests;


namespace Api.FarmManagement.Controllers
{
    public class PremiseManagementControllers
    {
        public static async Task<IResult> CreatePremise(IPremiseManagementRepository repo, [FromBody] PremiseCreationRequest request, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "you must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create Premise with name: {FarmName} for user ID: {UserId}", request.Name, userId);

                Premise createdFarm = await repo.CreatePremise(request);

                Log.Information("Premise created successfully with ID: {premisesId} for user ID: {UserId}", createdFarm.PremisesId, userId);
                return Results.Created($"/premises/{createdFarm.PremisesId}", createdFarm);
            }
            catch (ItemAlreadyExistsException ex)
            {
                Log.Warning(ex, "Premise creation failed - Premise already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the Premise.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetAllPremisesAsync(IPremiseManagementRepository repo, int pageNumber, int pageSize, string? search = null, int? ownerId = null)
        {
            try
            {
                Log.Information("Attempting to retrieve premises with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                var pagedResult = await repo.GetAllPremisesAsync(pageNumber, pageSize, search,ownerId);
                if (pagedResult == null)
                {
                    return Results.NotFound(new { message = "no premises found" });
                }
                Log.Information("Successfully retrieved {FarmCount} premises out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving premises.");
                return Results.Problem(ex.Message);
            }
        }


        public static async Task<IResult> GetAllPremisesByUserIdAsync(IPremiseManagementRepository repo, int userId,string? agent="no")
        {
            try
            {
                Log.Information("Attempting to retrieve premises by userId: userId {userId}.", userId);

                var premises = await repo.GetPremisesByUserId(userId,agent);
                if (premises == null)
                {
                    return Results.NotFound(new { message = "no premises found" });
                }
                
                return Results.Ok(premises);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving premises.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetPremiseByIdAsync(IPremiseManagementRepository repo, int premisesId)
        {
            try
            {
                Log.Information("Attempting to retrieve Premise with ID: {premisesId}", premisesId);

                var Premise = await repo.GetPremiseByIdAsync(premisesId);

                if (Premise == null)
                {
                    Log.Warning("Premise with ID: {premisesId} not found.", premisesId);
                    return Results.NotFound(new { message = "Premise not found." });
                }

                Log.Information("Successfully retrieved Premise with ID: {premisesId}", premisesId);
                return Results.Ok(Premise);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving Premise details.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdatePremise(IPremiseManagementRepository repo, PremiseUpdateRequest request)
        {
            try
            {
                
                Premise updatedFarm = await repo.UpdatePremise(request);

                
                Log.Information("Premise updated successfully with ID: {premisesId}", updatedFarm.PremisesId);
                return Results.Ok(updatedFarm);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Premise update failed - Premise does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the Premise.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeletePremise(IPremiseManagementRepository repo, int premisesId)
        {
            try
            {
                
                bool deleted = await repo.DeletePremise(premisesId);

                if (deleted)
                {
                    
                    Log.Information("Premise deleted successfully with ID: {premisesId}", premisesId);
                    return Results.Ok("Premise deleted Successfully"); 
                }
                else
                {
                    
                    return Results.NotFound(new { message = $"Premise with ID {premisesId} does not exist." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the Premise.");
                return Results.Problem(ex.Message);
            }
        }

      
        public static async Task<IResult> CountPremises(IPremiseManagementRepository repo)
        {
            try
            {

                int farmCount = await repo.CountPremisesAsync();

               
                if (farmCount == 0)
                {
                    return Results.NotFound(new { message = "No Premise records found for the specified criteria." });
                }

                return Results.Ok(new { count = farmCount });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting premises records.");
                return Results.Problem(ex.Message);
            }
        }








    }
}
