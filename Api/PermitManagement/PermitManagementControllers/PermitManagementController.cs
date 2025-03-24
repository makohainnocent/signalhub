
using DataAccess.Common.Exceptions;
using DataAccess.PermitManagement.Repositories;
using Domain.Core.Models;
using Domain.PermitManagement.Requests;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Api.PermitManagement.PermitManagementControllers
{
    public class PermitManagementController
    {
        public static async Task<IResult> CreatePermitApplication(IPermitRepository repo, [FromBody] PermitApplicationCreationRequest request, HttpContext httpContext)
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
                Log.Information("Attempting to create permit application for applicant ID: {ApplicantId} and user ID: {UserId}", request.ApplicantId, userId);

                //request.AppliedBy = userId;
                // Create the application
                PermitApplication createdApplication = await repo.CreatePermitApplication(request);

                // Log successful creation
                Log.Information("Permit application created successfully with ID: {ApplicationId} for applicant ID: {ApplicantId} and user ID: {UserId}", createdApplication.ApplicationId, request.ApplicantId, userId);

                // Return HTTP 201 Created with the newly created application
                return Results.Created($"/permit-applications/{createdApplication.ApplicationId}", createdApplication);
            }
            catch (ItemAlreadyExistsException ex)
            {
                // Handle case where the application already exists
                Log.Warning(ex, "Permit application creation failed - application already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Log.Error(ex, "An error occurred while creating the permit application.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetAllPermitApplicationsAsync(
            IPermitRepository repo,
            int pageNumber,
            int pageSize,
            string? search = null,
            int? userId = null,
            int? permitId = null,
            string? type = null,
            string? agent = "no")
        {
            try
            {
                Log.Information("Attempting to retrieve permit applications with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                var pagedResult = await repo.GetAllPermitApplicationsAsync(pageNumber, pageSize, search, userId, permitId, type,agent);
                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No permit applications found" });
                }

                Log.Information("Successfully retrieved {applicationCount} permit applications out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving permit applications.");
                return Results.Problem("An unexpected error occurred while retrieving permit applications.");
            }
        }

        public static async Task<IResult> GetPermitApplicationByIdAsync(IPermitRepository repo, int applicationId)
        {
            try
            {
                Log.Information("Attempting to retrieve permit application with ID: {ApplicationId}", applicationId);

                var application = await repo.GetPermitApplicationByIdAsync(applicationId);

                if (application == null)
                {
                    Log.Warning("Permit application with ID: {ApplicationId} not found.", applicationId);
                    return Results.NotFound(new { message = "Permit application not found." });
                }

                Log.Information("Successfully retrieved permit application with ID: {ApplicationId}", applicationId);
                return Results.Ok(application);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving permit application details.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdatePermitApplication(IPermitRepository repo, PermitApplicationUpdateRequest request)
        {
            try
            {
                PermitApplication updatedApplication = await repo.UpdatePermitApplication(request);

                Log.Information("Permit application updated successfully with ID: {ApplicationId}", updatedApplication.ApplicationId);
                return Results.Ok(updatedApplication);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Permit application update failed - application does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the permit application.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeletePermitApplication(IPermitRepository repo, int applicationId)
        {
            try
            {
                bool deleted = await repo.DeletePermitApplication(applicationId);

                if (deleted)
                {
                    Log.Information("Permit application deleted successfully with ID: {ApplicationId}", applicationId);
                    return Results.Ok("Permit application deleted successfully");
                }
                else
                {
                    return Results.NotFound(new { message = $"Permit application with ID {applicationId} does not exist." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the permit application.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountPermitApplicationsAsync(IPermitRepository repo, int? applicantId = null, int? applicantType = null)
        {
            try
            {
                int applicationCount = await repo.CountPermitApplicationsAsync(applicantId, applicantType);

                if (applicationCount == 0)
                {
                    return Results.NotFound(new { message = "No permit applications found for the specified criteria." });
                }

                return Results.Ok(new { count = applicationCount });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting permit applications.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountPendingPermitApplicationsAsync(IPermitRepository repo)
        {
            try
            {
                int applicationCount = await repo.CountPendingApplicationsAsync();

                if (applicationCount == 0)
                {
                    return Results.NotFound(new { message = "No pending permit applications found." });
                }

                return Results.Ok(new { count = applicationCount });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting pending permit applications.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CreatePermitAsync(IPermitRepository repo, [FromBody] PermitCreationRequest request)
        {
            try
            {
                Log.Information("Attempting to create a new permit with name: {PermitName}", request.PermitName);

                int permitId = await repo.CreatePermitAsync(request);

                Log.Information("Permit created successfully with ID: {PermitId}", permitId);
                return Results.Created($"/permits/{permitId}", new { PermitId = permitId });
            }
            catch (ItemAlreadyExistsException ex)
            {
                Log.Warning(ex, "Permit creation failed - permit already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the permit.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetPermitByIdAsync(IPermitRepository repo, int permitId)
        {
            try
            {
                Log.Information("Attempting to retrieve permit with ID: {PermitId}", permitId);

                var permit = await repo.GetPermitByIdAsync(permitId);

                if (permit == null)
                {
                    Log.Warning("Permit with ID: {PermitId} not found.", permitId);
                    return Results.NotFound(new { message = "Permit not found." });
                }

                Log.Information("Successfully retrieved permit with ID: {PermitId}", permitId);
                return Results.Ok(permit);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the permit.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdatePermitAsync(IPermitRepository repo, PermitUpdateRequest request)
        {
            try
            {
                Log.Information("Attempting to update permit with ID: {PermitId}", request.PermitId);

                var updatedPermit = await repo.UpdatePermitAsync(request);

                Log.Information("Permit updated successfully with ID: {PermitId}", request.PermitId);
                return Results.Ok(updatedPermit);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Permit update failed - permit does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the permit.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeletePermitAsync(IPermitRepository repo, int permitId)
        {
            try
            {
                Log.Information("Attempting to delete permit with ID: {PermitId}", permitId);

                bool deleted = await repo.DeletePermitAsync(permitId);

                if (deleted)
                {
                    Log.Information("Permit deleted successfully with ID: {PermitId}", permitId);
                    return Results.Ok("Permit deleted successfully");
                }
                else
                {
                    Log.Warning("Permit with ID: {PermitId} not found.", permitId);
                    return Results.NotFound(new { message = $"Permit with ID {permitId} does not exist." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the permit.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountPermitsAsync(IPermitRepository repo, string? permitName = null)
        {
            try
            {
                Log.Information("Attempting to count permits with filter: PermitName = {PermitName}", permitName);

                int count = await repo.CountPermitsAsync(permitName);

                Log.Information("Successfully counted permits: {Count}", count);
                return Results.Ok(new { Count = count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting permits.");
                return Results.Problem(ex.Message);
            }
        }


        public static async Task<IResult> GetAllPermitsAsync(
            IPermitRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            try
            {
                Log.Information("Attempting to retrieve permits with pagination: Page {PageNumber}, PageSize {PageSize}, Search: {Search}", pageNumber, pageSize, search);

                var pagedResult = await repo.GetAllPermitsAsync(pageNumber, pageSize, search);
                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No permits found" });
                }

                Log.Information("Successfully retrieved {permitCount} permits out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving permits.");
                return Results.Problem("An unexpected error occurred while retrieving permits.");
            }
        }
    }
}