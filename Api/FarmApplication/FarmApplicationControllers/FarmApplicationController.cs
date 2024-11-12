using Application.Application.Abstractions;
using Application.FarmManagement.Abstractions;
using Application.LivestockManagement.Abstractions;
using DataAccess.Common.Exceptions;
using Domain.Core.Models;
using Domain.FarmApplication.Requests;
using Domain.FarmManagement.Requests;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Api.FarmApplication.FarmApplicationControllers
{
    public class FarmApplicationController
    {
        public static async Task<IResult> CreateApplication(IApplicationRepository repo, [FromBody] ApplicationCreationRequest request, HttpContext httpContext)
        {
            try
            {
                // Extract the user ID from the claims in the HttpContext
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                // Log the application creation attempt
                Log.Information("Attempting to create application for farm ID: {FarmId} and user ID: {UserId}", request.FarmId, userId);

                request.UserId = userId;
                // Create the application
                FarmApplicationModel createdApplication = await repo.CreateApplication(request);

                // Log successful creation
                Log.Information("Application created successfully with ID: {ApplicationId} for farm ID: {FarmId} and user ID: {UserId}", createdApplication.ApplicationId, request.FarmId, userId);

                // Return HTTP 201 Created with the newly created application
                return Results.Created($"/applications/{createdApplication.ApplicationId}", createdApplication);
            }
            catch (ItemAlreadyExistsException ex)
            {
                // Handle case where the application already exists
                Log.Warning(ex, "Application creation failed - application already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Log.Error(ex, "An error occurred while creating the application.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetAllFarmApplicationsAsync(
     IApplicationRepository repo,
     int pageNumber,
     int pageSize,
     string? search = null,
     int? userId = null,
     int? farmId = null,
     string? type=null)
        {
            try
            {
                Log.Information("Attempting to retrieve applications with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                var pagedResult = await repo.GetAllApplicationsAsync(pageNumber, pageSize, search, userId, farmId,type);
                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No applications found" });
                }

                Log.Information("Successfully retrieved {applicationCount} applications out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving applications.");
                return Results.Problem("An unexpected error occurred while retrieving applications.");
            }
        }

        public static async Task<IResult> GetFarmApplicationByIdAsync(IApplicationRepository repo, int applicationId)
        {
            try
            {
                Log.Information("Attempting to retrieve application with ID: {ApplicationId}", applicationId);

                var application = await repo.GetApplicationByIdAsync(applicationId);

                if (application == null)
                {
                    Log.Warning("Application with ID: {applicationId} not found.", applicationId);
                    return Results.NotFound(new { message = "application not found." });
                }

                Log.Information("Successfully retrieved application with ID: {applicationId}", applicationId);
                return Results.Ok(application);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving application details.");
                return Results.Problem(ex.Message);
            }
        }


        public static async Task<IResult> UpdateApplication(IApplicationRepository repo, ApplicationUpdateRequest request)
        {
            try
            {

                FarmApplicationModel updatedApplication = await repo.UpdateApplication(request);


                Log.Information("Application updated successfully with ID: {applicationId}", updatedApplication.FarmId);
                return Results.Ok(updatedApplication);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Application update failed - application does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the farm.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteApplication(IApplicationRepository repo, int applicationId)
        {
            try
            {

                bool deleted = await repo.DeleteApplication(applicationId);

                if (deleted)
                {

                    Log.Information("Application deleted successfully with ID: {applicationId}", applicationId);
                    return Results.Ok("application deleted Successfully");
                }
                else
                {

                    return Results.NotFound(new { message = $"application with ID {applicationId} does not exist." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the application.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountApplications(IApplicationRepository repo,int? userId = null,int? farmId = null)
        {
            try
            {
                // Call repository method to get livestock count based on userId or farmId
                int applicationCount = await repo.CountApplicationsAsync(userId, farmId);

                // Check if any applicationCount records exist
                if (applicationCount == 0)
                {
                    return Results.NotFound(new { message = "No applicationCount records found for the specified criteria." });
                }

                return Results.Ok(new { count = applicationCount });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting applicationCount records.");
                return Results.Problem(ex.Message);
            }
        }

    }
}
