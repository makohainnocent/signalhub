// Api/UserDevices/EndPointDefinations/UserDevicesEndpoints.cs
using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.UserDevices.Abstractions;
using Domain.UserDevices.Requests;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Api.UserDevices.Controllers;

namespace Api.UserDevices.EndPointDefinations
{
    public class UserDevicesEndpoints : IEndpointDefinition
    {
        public void RegisterEndpoints(WebApplication app)
        {
            ApiVersionSet apiVersionSet = app.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1))
                .ReportApiVersions()
                .Build();

            RouteGroupBuilder versionedGroup = app
                .MapGroup("/api/v{apiVersion:apiVersion}")
                .WithApiVersionSet(apiVersionSet);

            var userDevices = versionedGroup.MapGroup("/user-devices")
                .WithTags("User Devices Management");

            // Create a new user device
            userDevices.MapPost("/", async (IUserDevicesRepository repo, [FromBody] UserDeviceCreationRequest request, HttpContext httpContext) =>
            {
                return await UserDevicesControllers.CreateUserDeviceAsync(repo, request, httpContext);
            })
            .RequireAuthorization();
            

            // Get all user devices (paginated)
            userDevices.MapGet("/", async (IUserDevicesRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null) =>
            {
                return await UserDevicesControllers.GetUserDevicesAsync(repo, pageNumber, pageSize, search);
            });

            // Get a user device by ID
            userDevices.MapGet("/{deviceId:int}", async (IUserDevicesRepository repo, int deviceId) =>
            {
                return await UserDevicesControllers.GetUserDeviceByIdAsync(repo, deviceId);
            });

            // Update a user device
            userDevices.MapPut("/", async (IUserDevicesRepository repo, [FromBody] UserDeviceUpdateRequest request) =>
            {
                return await UserDevicesControllers.UpdateUserDeviceAsync(repo, request);
            })
            .RequireAuthorization();
            

            // Delete a user device
            userDevices.MapDelete("/{deviceId:int}", async (IUserDevicesRepository repo, int deviceId) =>
            {
                return await UserDevicesControllers.DeleteUserDeviceAsync(repo, deviceId);
            })
            .RequireAuthorization();

            // Count all user devices
            userDevices.MapGet("/count", async (IUserDevicesRepository repo) =>
            {
                return await UserDevicesControllers.CountUserDevicesAsync(repo);
            });
        }
    }
}