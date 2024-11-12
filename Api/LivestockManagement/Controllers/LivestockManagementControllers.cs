using Application.LivestockManagement.Abstractions;
using DataAccess.Common.Exceptions;
using Domain.Core.Models;
using Domain.LivestockManagement.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Api.LivestockManagement.Controllers
{
    public class LivestockManagementControllers
    {
            public static async Task<IResult> CreateLivestock(ILivestockManagementRepository repo, [FromBody] LivestockCreationRequest request, HttpContext httpContext)
            {
                try
                {
                    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    {
                        return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                    }

                    Log.Information("Attempting to create livestock for user ID: {UserId}", userId);

                    Livestock createdLivestock = await repo.CreateLivestock(request, userId);

                    Log.Information("Livestock created successfully with ID: {LivestockId} for user ID: {UserId}", createdLivestock.LivestockId, userId);
                    return Results.Created($"/livestock/{createdLivestock.LivestockId}", createdLivestock);
                }
                catch (ItemAlreadyExistsException ex)
                {
                    Log.Warning(ex, "Livestock creation failed - livestock already exists.");
                    return Results.Conflict(new { message = ex.Message });
                }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, ex.Message);
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while creating the livestock.");
                    return Results.Problem(ex.Message);
                }
            }


        public static async Task<IResult> GetLivestockByFarm(ILivestockManagementRepository repo, int farmId, int pageNumber, int pageSize, string? search = null)
        {
            try
            {
                var livestockPagedResult = await repo.GetLivestockByFarmAsync(farmId, pageNumber, pageSize, search);

              
                if (livestockPagedResult == null || !livestockPagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No livestock records found for the specified farm." });
                }

                return Results.Ok(livestockPagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving livestock records for the specified farm.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountLivestock(
    ILivestockManagementRepository repo,
    int? userId = null,
    int? farmId = null)
        {
            try
            {
                // Call repository method to get livestock count based on userId or farmId
                int livestockCount = await repo.CountLivestockAsync(userId, farmId);

                // Check if any livestock records exist
                if (livestockCount == 0)
                {
                    return Results.NotFound(new { message = "No livestock records found for the specified criteria." });
                }

                return Results.Ok(new { count = livestockCount });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting livestock records.");
                return Results.Problem(ex.Message);
            }
        }



        public static async Task<IResult> GetLivestockById(ILivestockManagementRepository repo, int livestockId)
        {
            try
            {
                var livestock = await repo.GetLivestockByIdAsync(livestockId);

                
                if (livestock == null)
                {
                    return Results.NotFound(new { message = "Livestock not found." });
                }

                return Results.Ok(livestock);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving livestock details.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateLivestock(ILivestockManagementRepository repo, LivestockUpdateRequest request, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to update livestock with ID: {LivestockId} for user ID: {UserId}", request.LivestockId, userId);

                var updatedLivestock = await repo.UpdateLivestockAsync(request, userId);

                Log.Information("Livestock updated successfully with ID: {LivestockId} for user ID: {UserId}", updatedLivestock.LivestockId, userId);
                return Results.Ok(updatedLivestock);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Livestock update failed - livestock does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating livestock.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteLivestock(ILivestockManagementRepository repo, int livestockId, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to delete livestock with ID: {LivestockId} for user ID: {UserId}", livestockId, userId);

                var result = await repo.DeleteLivestockAsync(livestockId, userId);

                if (result)
                {
                    Log.Information("Livestock deleted successfully with ID: {LivestockId} for user ID: {UserId}", livestockId, userId);
                    return Results.Ok("Livestock deleted successfully");
                }
                else
                {
                    Log.Warning("Livestock deletion failed - livestock does not exist.");
                    return Results.NotFound(new { message = "Livestock not found." });
                }
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Livestock deletion failed - livestock does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting livestock.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CreateHealthRecord(HealthRecordCreationRequest request, ILivestockManagementRepository repo, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create health record for user ID: {UserId}", request.UserId);

                request.UserId = userId;

                var createdHealthRecord = await repo.CreateHealthRecordAsync(request);

                Log.Information("Health record created successfully with ID: {HealthRecordId} for user ID: {UserId}", createdHealthRecord.HealthRecordId, request.UserId);
                return Results.Created($"/healthrecords/{createdHealthRecord.HealthRecordId}", createdHealthRecord);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, ex.Message);
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the health record.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetHealthRecordsByLivestock(
        ILivestockManagementRepository repo,
        int livestockId,
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null)
        {
            try
            {
                var healthRecordsPagedResult = await repo.GetHealthRecordsByLivestockIdAsync(livestockId, pageNumber, pageSize, search);

                if (healthRecordsPagedResult == null || !healthRecordsPagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No health records found for the specified livestock." });
                }

                return Results.Ok(healthRecordsPagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving health records for the specified livestock.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetHealthRecordById(ILivestockManagementRepository repo, int healthRecordId)
        {
            try
            {
                var healthRecord = await repo.GetHealthRecordByIdAsync(healthRecordId);

                if (healthRecord == null)
                {
                    return Results.NotFound(new { message = "Health record not found." });
                }

                return Results.Ok(healthRecord);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the health record.");
                return Results.Problem(ex.Message);
            }
        }



        public static async Task<IResult> UpdateHealthRecord(ILivestockManagementRepository repo, int healthRecordId, UpdateHealthRecordRequest updatedHealthRecord)
        {
            try
            {
                
                var existingHealthRecord = await repo.GetHealthRecordByIdAsync(healthRecordId);
                if (existingHealthRecord == null)
                {
                    return Results.NotFound(new { message = "Health record not found." });
                }

                
                var isUpdated = await repo.UpdateHealthRecordAsync(healthRecordId, updatedHealthRecord);

                if (isUpdated)
                {
                    return Results.Ok(new { message = "Health record updated successfully." });
                }
                else
                {
                    return Results.Problem("An error occurred while updating the health record.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the health record.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteHealthRecord(ILivestockManagementRepository repo, int healthRecordId)
        {
            try
            {
                var existingHealthRecord = await repo.GetHealthRecordByIdAsync(healthRecordId);
                if (existingHealthRecord == null)
                {
                    return Results.NotFound(new { message = "Health record not found." });
                }

                var isDeleted = await repo.DeleteHealthRecordAsync(healthRecordId);

                if (isDeleted)
                {
                    return Results.Ok(new { message = "Health record deleted successfully." });
                }
                else
                {
                    return Results.Problem("An error occurred while deleting the health record.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the health record.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CreateDirective(ILivestockManagementRepository repo, CreateDirectiveRequest newDirective)
        {
            try
            {
                var directiveId = await repo.CreateDirectiveAsync(newDirective);

                return Results.Created($"/directive/{directiveId}", new { message = "Directive created successfully.", DirectiveId = directiveId });
            }
            catch (ItemDoesNotExistException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the directive.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetDirectivesByLivestock(ILivestockManagementRepository repo, int livestockId, int pageNumber = 1, int pageSize = 10, string? search = null)
        {
            try
            {
               
                var livestock = await repo.GetLivestockByIdAsync(livestockId);
                if (livestock == null)
                {
                    return Results.NotFound(new { message = "Livestock not found." });
                }

                var directivesPagedResult = await repo.GetDirectivesByLivestockAsync(livestockId, pageNumber, pageSize, search);

                if (directivesPagedResult == null || !directivesPagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No directives found for the specified livestock." });
                }

                return Results.Ok(directivesPagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving directives for the specified livestock.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetDirectiveById(ILivestockManagementRepository repo, int directiveId)
        {
            try
            {
                var directive = await repo.GetDirectiveByIdAsync(directiveId);

                if (directive == null)
                {
                    return Results.NotFound(new { message = "Directive not found." });
                }

                return Results.Ok(directive);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the directive.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateDirective(ILivestockManagementRepository repo, int directiveId, UpdateDirectiveRequest updateDirective)
        {
            try
            {
                
                var existingDirective = await repo.GetDirectiveByIdAsync(directiveId);
                if (existingDirective == null)
                {
                    return Results.NotFound(new { message = "Directive not found." });
                }

                
                var isUpdated = await repo.UpdateDirectiveDetailsAsync(directiveId, updateDirective.DirectiveDetails);

                if (isUpdated)
                {
                    return Results.Ok(new { message = "Directive updated successfully." });
                }
                else
                {
                    return Results.Problem("An error occurred while updating the directive.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the directive.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteDirective(ILivestockManagementRepository repo, int directiveId)
        {
            try
            {
                
                var existingDirective = await repo.GetDirectiveByIdAsync(directiveId);
                if (existingDirective == null)
                {
                    return Results.NotFound(new { message = "Directive not found." });
                }

               
                var isDeleted = await repo.DeleteDirectiveAsync(directiveId);

                if (isDeleted)
                {
                    return Results.Ok(new { message = "Directive deleted successfully." });
                }
                else
                {
                    return Results.Problem("An error occurred while deleting the directive.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the directive.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountHealthRecords(
            ILivestockManagementRepository repo, int? userId = null, int? livestockId = null, int? farmId = null)


        {
            try
            {
                // Call repository method to get livestock count based on userId or farmId
                int livestockCount = await repo.CountHealthRecordsAsync(userId,livestockId, farmId);

                // Check if any livestock records exist
                if (livestockCount == 0)
                {
                    return Results.NotFound(new { message = "No health records found for the specified criteria." });
                }

                return Results.Ok(new { count = livestockCount });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting health records.");
                return Results.Problem(ex.Message);
            }
        }








    }
}
