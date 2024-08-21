using Application.Authentication.Abstractions;
using DataAccess.Authentication.Exceptions;
using DataAccess.Authentication.Utilities;
using Domain.Authentication.Requests;
using Domain.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Serilog;
using Api.Authentication.Utilities;
using Application.Common.Abstractions;
using System.Security.Claims;
using Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using DataAccess.Common.Exceptions;
using Domain.FarmManagement.Requests;
using Application.FarmManagement.Abstractions;

namespace Api.FarmManagement.Controllers
{
    public class FarmManagementControllers
    {
        public static async Task<IResult> CreateFarm(IFarmManagementRepository repo, [FromBody] FarmCreationRequest request, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "you must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create farm with name: {FarmName} for user ID: {UserId}", request.FarmName, userId);

                Farm createdFarm = await repo.CreateFarm(request, userId);

                Log.Information("Farm created successfully with ID: {FarmId} for user ID: {UserId}", createdFarm.FarmId, userId);
                return Results.Created($"/farms/{createdFarm.FarmId}", createdFarm);
            }
            catch (ItemAlreadyExistsException ex)
            {
                Log.Warning(ex, "Farm creation failed - farm already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the farm.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetAllFarmsAsync(IFarmManagementRepository repo, int pageNumber, int pageSize, string? search = null)
        {
            try
            {
                Log.Information("Attempting to retrieve farms with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                var pagedResult = await repo.GetAllFarmsAsync(pageNumber, pageSize, search);
                if (pagedResult == null)
                {
                    return Results.NotFound(new { message = "no farms found" });
                }
                Log.Information("Successfully retrieved {FarmCount} farms out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving farms.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetFarmByIdAsync(IFarmManagementRepository repo, int farmId)
        {
            try
            {
                Log.Information("Attempting to retrieve farm with ID: {FarmId}", farmId);

                var farm = await repo.GetFarmByIdAsync(farmId);

                if (farm == null)
                {
                    Log.Warning("Farm with ID: {FarmId} not found.", farmId);
                    return Results.NotFound(new { message = "Farm not found." });
                }

                Log.Information("Successfully retrieved farm with ID: {FarmId}", farmId);
                return Results.Ok(farm);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving farm details.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateFarm(IFarmManagementRepository repo, FarmUpdateRequest request)
        {
            try
            {
                
                Farm updatedFarm = await repo.UpdateFarm(request);

                
                Log.Information("Farm updated successfully with ID: {FarmId}", updatedFarm.FarmId);
                return Results.Ok(updatedFarm);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Farm update failed - farm does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the farm.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteFarm(IFarmManagementRepository repo, int farmId)
        {
            try
            {
                
                bool deleted = await repo.DeleteFarm(farmId);

                if (deleted)
                {
                    
                    Log.Information("Farm deleted successfully with ID: {FarmId}", farmId);
                    return Results.Ok("Farm deleted Successfully"); 
                }
                else
                {
                    
                    return Results.NotFound(new { message = $"Farm with ID {farmId} does not exist." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the farm.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CreateFarmGeofencing(IFarmManagementRepository repo, FarmGeofencingRequest request)
        {
            try
            {
                Log.Information("Attempting to create geofencing for farm ID: {FarmId}", request.FarmId);

                FarmGeofencing createdGeofencing = await repo.CreateFarmGeofencing(request);

                Log.Information("Farm geofencing created successfully with ID: {GeofenceId}", createdGeofencing.GeofenceId);
                return Results.Created($"/farms/geofencing/{createdGeofencing.GeofenceId}", createdGeofencing);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Geofencing creation failed - farm does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating farm geofencing.");
                return Results.Problem(ex.Message);
            }
        }

       
            public static async Task<IResult> GetAllFarmGeofencings(IFarmManagementRepository repo, int pageNumber, int pageSize, string? search)
            {
                try
                {
                    var farmGeofencings = await repo.GetAllFarmGeofencingsAsync(pageNumber, pageSize, search);
                    return Results.Ok(farmGeofencings);
                }
                catch (Exception ex)
                {
                    
                    return Results.Problem("An error occurred while retrieving farm geofencings.");
                }
            }


        public static async Task<IResult> GetMostRecentGeofenceByFarmId(IFarmManagementRepository repo, int farmId)
        {
            try
            {
                var geofence = await repo.GetMostRecentGeofenceByFarmIdAsync(farmId);
                if (geofence == null)
                {
                    return Results.NotFound(new { message = $"No geofence found for Farm ID {farmId}." });
                }

                return Results.Ok(geofence);
            }
            catch (Exception ex)
            {

                return Results.Problem("An error occurred while retrieving the geofence.");
            }
        }

        public static async Task<IResult> UpdateGeofence(IFarmManagementRepository repo, FarmGeofencingUpdateRequest request)
        {
            try
            {
                
                if (request.GeofenceId <= 0)
                {
                    return Results.BadRequest(new { message = "Invalid Geofence ID." });
                }

                var updatedGeofence = await repo.UpdateGeofenceAsync(request);

                return Results.Ok(updatedGeofence);
            }
            catch (ItemDoesNotExistException ex)
            {
                
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                
                return Results.Problem("An error occurred while updating the geofence.");
            }
        }

        public static async Task<IResult> DeleteGeofence(IFarmManagementRepository repo, int geofenceId)
        {
            try
            {
                if (geofenceId <= 0)
                {
                    return Results.BadRequest(new { message = "Invalid Geofence ID." });
                }

                var deleted = await repo.DeleteGeofenceAsync(geofenceId);

                if (deleted)
                {
                    return Results.Ok("geofence deleted successfully");
                }
                else
                {
                    return Results.NotFound(new { message = "Geofence not found." });
                }
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while deleting the geofence.");
            }
        }








    }
}
