using Application.LivestockManagement.Abstractions;
using Application.Vaccinations.Abstractions;
using Domain.Common.Responses;
using Domain.Core.Models.Domain.Core.Models;
using Domain.Vaccinations.Requests;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Api.Vaccinations.VaccinationsControllers
{
    public class VaccinationControllers
    {
        public static async Task<IResult> CreateVaccinationAsync(IVaccinationRepository repo, [FromBody] CreateVaccinationRequest request, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create vaccination for livestock ID: {LivestockId} by user ID: {UserId}", request.LivestockId, userId);

                request.UserId = userId;
                Vaccination createdVaccination = await repo.CreateVaccinationAsync(request);

                Log.Information("Vaccination created successfully with ID: {VaccinationId} by user ID: {UserId}", createdVaccination.VaccinationId, userId);
                return Results.Created($"/vaccinations/{createdVaccination.VaccinationId}", createdVaccination);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the vaccination.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetAllVaccinationsAsync(IVaccinationRepository repo, int pageNumber, int pageSize, int? farmId = null,int? userId=null ,string? search = null)
        {
            try
            {
                Log.Information("Attempting to retrieve vaccinations with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                PagedResultResponse<Vaccination> pagedResult = await repo.GetAllVaccinationsAsync(pageNumber, pageSize,farmId,userId, search);

                if (pagedResult.Items == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No vaccinations found." });
                }

                Log.Information("Successfully retrieved {VaccinationCount} vaccinations out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving vaccinations.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetVaccinationByIdAsync(IVaccinationRepository repo, int vaccinationId)
        {
            try
            {
                Log.Information("Attempting to retrieve vaccination with ID: {VaccinationId}", vaccinationId);

                Vaccination? vaccination = await repo.GetVaccinationByIdAsync(vaccinationId);

                if (vaccination == null)
                {
                    Log.Warning("Vaccination with ID: {VaccinationId} not found.", vaccinationId);
                    return Results.NotFound(new { message = "Vaccination not found." });
                }

                Log.Information("Successfully retrieved vaccination with ID: {VaccinationId}", vaccinationId);
                return Results.Ok(vaccination);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving vaccination details.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateVaccinationAsync(IVaccinationRepository repo, int vaccinationId, [FromBody] Vaccination vaccination)
        {
            try
            {
                Log.Information("Attempting to update vaccination with ID: {VaccinationId}", vaccinationId);

                Vaccination updatedVaccination = await repo.UpdateVaccinationAsync(vaccination);

                Log.Information("Vaccination updated successfully with ID: {VaccinationId}", updatedVaccination.VaccinationId);
                return Results.Ok(updatedVaccination);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the vaccination.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteVaccinationAsync(IVaccinationRepository repo, int vaccinationId)
        {
            try
            {
                Log.Information("Attempting to delete vaccination with ID: {VaccinationId}", vaccinationId);

                bool deleted = await repo.DeleteVaccinationAsync(vaccinationId);

                if (deleted)
                {
                    Log.Information("Vaccination deleted successfully with ID: {VaccinationId}", vaccinationId);
                    return Results.Ok("Vaccination deleted successfully.");
                }
                else
                {
                    return Results.NotFound(new { message = $"Vaccination with ID {vaccinationId} does not exist." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the vaccination.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountVaccinations(
   IVaccinationRepository repo,
   int? userId = null,
   int? farmId = null, int? livestockId = null)
        {
            try
            {
                
                int count = await repo.CountVaccinationsAsync(userId, farmId,livestockId);

                
                if (count == 0)
                {
                    return Results.NotFound(new { message = "No vaccination records found for the specified criteria." });
                }

                return Results.Ok(new { count = count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting vaccination records.");
                return Results.Problem(ex.Message);
            }
        }
    }
}
