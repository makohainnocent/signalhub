using Application.AnimalManagement.Abstractions;
using DataAccess.Common.Exceptions;
using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.AnimalManagement.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Api.AnimalManagement.Controllers
{
    public class AnimalManagementControllers
    {
        public static async Task<IResult> CreateAnimalAsync(IAnimalManagementRepository repo, [FromBody] AnimalCreationRequest request, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create animal for user ID: {UserId}", userId);

                Animal createdAnimal = await repo.CreateAnimalAsync(request);

                Log.Information("Animal created successfully with ID: {AnimalId} for user ID: {UserId}", createdAnimal.AnimalId, userId);
                return Results.Created($"/animals/{createdAnimal.AnimalId}", createdAnimal);
            }
            catch (ItemAlreadyExistsException ex)
            {
                Log.Warning(ex, "Animal creation failed - animal already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, ex.Message);
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the animal.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetAnimalsByPremisesAsync(IAnimalManagementRepository repo, int premisesId, int pageNumber, int pageSize, string? search = null)
        {
            try
            {
                var animalsPagedResult = await repo.GetAnimalsByPremisesAsync(premisesId, pageNumber, pageSize, search);

                if (animalsPagedResult == null || !animalsPagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No animals found for the specified premises." });
                }

                return Results.Ok(animalsPagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving animals for the specified premises.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetAnimalByIdAsync(IAnimalManagementRepository repo, int animalId)
        {
            try
            {
                var animal = await repo.GetAnimalByIdAsync(animalId);

                if (animal == null)
                {
                    return Results.NotFound(new { message = "Animal not found." });
                }

                return Results.Ok(animal);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving animal details.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateAnimalAsync(IAnimalManagementRepository repo, AnimalUpdateRequest request, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to update animal with ID: {AnimalId} for user ID: {UserId}", request.AnimalId, userId);

                var updatedAnimal = await repo.UpdateAnimalAsync(request);

                Log.Information("Animal updated successfully with ID: {AnimalId} for user ID: {UserId}", updatedAnimal.AnimalId, userId);
                return Results.Ok(updatedAnimal);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Animal update failed - animal does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the animal.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteAnimalAsync(IAnimalManagementRepository repo, int animalId, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to delete animal with ID: {AnimalId} for user ID: {UserId}", animalId, userId);

                var result = await repo.DeleteAnimalAsync(animalId);

                if (result)
                {
                    Log.Information("Animal deleted successfully with ID: {AnimalId} for user ID: {UserId}", animalId, userId);
                    return Results.Ok("Animal deleted successfully");
                }
                else
                {
                    Log.Warning("Animal deletion failed - animal does not exist.");
                    return Results.NotFound(new { message = "Animal not found." });
                }
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Animal deletion failed - animal does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the animal.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountAnimalsAsync(IAnimalManagementRepository repo, int? ownerId = null, int? premisesId = null)
        {
            try
            {
                int animalCount = await repo.CountAnimalsAsync(ownerId, premisesId);

                if (animalCount == 0)
                {
                    return Results.NotFound(new { message = "No animals found for the specified criteria." });
                }

                return Results.Ok(new { count = animalCount });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting animals.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CreateHealthRecordAsync(IAnimalManagementRepository repo, HealthRecordCreationRequest request, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create health record for user ID: {UserId}", userId);

                request.UserId = userId;

                var createdHealthRecord = await repo.CreateHealthRecordAsync(request);

                Log.Information("Health record created successfully with ID: {HealthRecordId} for user ID: {UserId}", createdHealthRecord.HealthRecordId, userId);
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

        public static async Task<IResult> GetHealthRecordsByAnimalIdAsync(IAnimalManagementRepository repo, int animalId, int pageNumber = 1, int pageSize = 10, string? search = null)
        {
            try
            {
                var healthRecordsPagedResult = await repo.GetHealthRecordsByAnimalIdAsync(animalId, pageNumber, pageSize, search);

                if (healthRecordsPagedResult == null || !healthRecordsPagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No health records found for the specified animal." });
                }

                return Results.Ok(healthRecordsPagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving health records for the specified animal.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetHealthRecordByIdAsync(IAnimalManagementRepository repo, int healthRecordId)
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

        public static async Task<IResult> UpdateHealthRecordAsync(IAnimalManagementRepository repo, int healthRecordId, UpdateHealthRecordRequest updateRequest)
        {
            try
            {
                var existingHealthRecord = await repo.GetHealthRecordByIdAsync(healthRecordId);
                if (existingHealthRecord == null)
                {
                    return Results.NotFound(new { message = "Health record not found." });
                }

                var isUpdated = await repo.UpdateHealthRecordAsync(healthRecordId, updateRequest);

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

        public static async Task<IResult> DeleteHealthRecordAsync(IAnimalManagementRepository repo, int healthRecordId)
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

        public static async Task<IResult> CountHealthRecordsAsync(IAnimalManagementRepository repo, int? userId = null, int? animalId = null, int? premisesId = null)
        {
            try
            {
                int healthRecordCount = await repo.CountHealthRecordsAsync(userId, animalId, premisesId);

                if (healthRecordCount == 0)
                {
                    return Results.NotFound(new { message = "No health records found for the specified criteria." });
                }

                return Results.Ok(new { count = healthRecordCount });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting health records.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetAllHealthRecordsAsync(IAnimalManagementRepository repo, int pageNumber = 1, int pageSize = 10, string? search = null, int? userId = null, int? animalId = null, int? premisesId = null)
        {
            try
            {
                Log.Information("Attempting to retrieve health records with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                var pagedResult = await repo.GetAllHealthRecordsAsync(pageNumber, pageSize, search, userId, animalId, premisesId);

                if (pagedResult.Items == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No health records found." });
                }

                Log.Information("Successfully retrieved {HealthRecordCount} health records out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving health records.");
                return Results.Problem("An error occurred while retrieving health records. Please try again later.");
            }
        }
    }
}