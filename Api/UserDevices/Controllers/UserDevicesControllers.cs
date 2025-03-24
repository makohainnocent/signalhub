// Api/UserDevices/Controllers/UserDevicesControllers.cs
using Domain.UserDevices.Models;
using Domain.UserDevices.Requests;
using Application.UserDevices.Abstractions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Claims;
using DataAccess.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.UserDevices.Controllers
{
    public static class UserDevicesControllers
    {
        public static async Task<IResult> CreateUserDeviceAsync(
            IUserDevicesRepository repo,
            [FromBody] UserDeviceCreationRequest request,
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

                Log.Information("Attempting to create user device for user ID: {UserId}", request.UserId);

                // Create the user device
                var createdUserDevice = await repo.CreateUserDeviceAsync(request);

                Log.Information("User device created successfully with ID: {DeviceId}", createdUserDevice.DeviceId);
                return Results.Created($"/user-devices/{createdUserDevice.DeviceId}", createdUserDevice);
            }
            catch (ItemAlreadyExistsException ex)
            {
                Log.Warning(ex, "User device creation failed - device token already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the user device.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetUserDevicesAsync(
            IUserDevicesRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            try
            {
                Log.Information("Attempting to retrieve user devices with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                // Retrieve paginated user devices
                var pagedResult = await repo.GetUserDevicesAsync(pageNumber, pageSize, search);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No user devices found." });
                }

                Log.Information("Successfully retrieved {DeviceCount} user devices out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving user devices.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetUserDeviceByIdAsync(
            IUserDevicesRepository repo,
            int deviceId)
        {
            try
            {
                Log.Information("Attempting to retrieve user device with ID: {DeviceId}", deviceId);

                // Retrieve the user device by ID
                var userDevice = await repo.GetUserDeviceByIdAsync(deviceId);

                if (userDevice == null)
                {
                    Log.Warning("User device with ID: {DeviceId} not found.", deviceId);
                    return Results.NotFound(new { message = "User device not found." });
                }

                Log.Information("Successfully retrieved user device with ID: {DeviceId}", deviceId);
                return Results.Ok(userDevice);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the user device.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateUserDeviceAsync(
            IUserDevicesRepository repo,
            [FromBody] UserDeviceUpdateRequest request)
        {
            try
            {
                Log.Information("Attempting to update user device with ID: {DeviceId}", request.DeviceId);

                // Update the user device
                var updatedUserDevice = await repo.UpdateUserDeviceAsync(request);

                Log.Information("User device updated successfully with ID: {DeviceId}", updatedUserDevice.DeviceId);
                return Results.Ok(updatedUserDevice);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "User device update failed - user device does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the user device.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteUserDeviceAsync(
            IUserDevicesRepository repo,
            int deviceId)
        {
            try
            {
                Log.Information("Attempting to delete user device with ID: {DeviceId}", deviceId);

                // Delete the user device
                var deleted = await repo.DeleteUserDeviceAsync(deviceId);

                if (deleted)
                {
                    Log.Information("User device deleted successfully with ID: {DeviceId}", deviceId);
                    return Results.Ok(new { message = "User device deleted successfully." });
                }
                else
                {
                    Log.Warning("User device deletion failed - user device does not exist.");
                    return Results.NotFound(new { message = "User device not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the user device.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> CountUserDevicesAsync(
            IUserDevicesRepository repo)
        {
            try
            {
                Log.Information("Attempting to count all user devices.");

                // Count the user devices
                var count = await repo.CountUserDevicesAsync();

                Log.Information("Successfully counted {DeviceCount} user devices.", count);
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting user devices.");
                return Results.Problem(ex.Message);
            }
        }
    }
}