using Api.Common.Abstractions;
using Api.Authentication.Controllers;
using Application.Authentication.Abstractions;
using Domain.Authentication.Requests;
using Api.Common.Filters;
using Api.Authentication.Validators;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning.Builder;
using Asp.Versioning;
using DataAccess.Authentication.Utilities;
using ForgotPasswordRequest = Domain.Authentication.Requests.ForgotPasswordRequest;
using ResetPasswordRequest = Domain.Authentication.Requests.ResetPasswordRequest;
using Application.Common.Abstractions;
using System.Security.Claims;
using Serilog;
using Microsoft.AspNetCore.Http;
using Domain.Core.Models;
using DataAccess.Authentication.Exceptions;
using System.Data;
using Domain.Authentication.Responses;
using Domain.FarmManagement.Requests;
using Application.FarmManagement.Abstractions;
using Api.FarmManagement.Controllers;
using System.Net;
using static Api.FarmManagement.Controllers.FarmManagementControllers;


namespace Api.FarmManagement.EndPointDefinations
{

    public class FarmManagementEndPoints : IEndpointDefinition
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

            var farm = versionedGroup.MapGroup("/farm");


            farm.MapPost("/farms", async (IFarmManagementRepository repo, [FromBody] FarmCreationRequest request, HttpContext httpContext) =>
            {
                
                return await FarmManagementControllers.CreateFarm(repo, request, httpContext);
            })
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<FarmCreationRequest>>()
            .WithTags("Farm Management");

            farm.MapGet("/farms-all", async (IFarmManagementRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null) =>
            {
                return await FarmManagementControllers.GetAllFarmsAsync(repo, pageNumber, pageSize, search);
            })
            .RequireAuthorization()
            .WithTags("Farm Management");

            farm.MapGet("/farms/{farmId}", async (IFarmManagementRepository repo, int farmId) =>
            {
                return await FarmManagementControllers.GetFarmByIdAsync(repo, farmId);
            })
            .RequireAuthorization()
            .WithTags("Farm Management");

            farm.MapPut("/farms", async (IFarmManagementRepository repo, [FromBody] FarmUpdateRequest request) =>
            {
                return await FarmManagementControllers.UpdateFarm(repo, request);
            })
             .RequireAuthorization()
             .AddEndpointFilter<ValidationFilter<FarmUpdateRequest>>()
             .WithTags("Farm Management");

            farm.MapDelete("/farms/{farmId}", async (IFarmManagementRepository repo, int farmId) =>
            {
                return await FarmManagementControllers.DeleteFarm(repo, farmId);
            })
            .RequireAuthorization()
            .WithTags("Farm Management");

            farm.MapPost("/geofencing", async (IFarmManagementRepository repo, [FromBody] FarmGeofencingRequest request) =>
            {
                return await FarmManagementControllers.CreateFarmGeofencing(repo, request);
            })
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<FarmGeofencingRequest>>()
            .WithTags("Farm Geofencing");

            farm.MapGet("/geofencing-all", async (IFarmManagementRepository repo, int pageNumber=1, int pageSize=10, string? search=null) =>
            {
                return await FarmManagementControllers.GetAllFarmGeofencings(repo, pageNumber, pageSize, search);
            })
           .WithTags("Farm Geofencing");

            farm.MapGet("/geofence/{farmId}", async (IFarmManagementRepository repo, int farmId) =>
            {
                return await FarmManagementControllers.GetMostRecentGeofenceByFarmId(repo, farmId);
            })
            .WithTags("Farm Geofencing");

            farm.MapPut("/geofence", async (IFarmManagementRepository repo, [FromBody] FarmGeofencingUpdateRequest request) =>
            {
                return await FarmManagementControllers.UpdateGeofence(repo, request);
            })
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<FarmGeofencingUpdateRequest>>()
            .WithTags("Farm Geofencing");

            farm.MapDelete("/geofence/{geofenceId}", async (IFarmManagementRepository repo, int geofenceId) =>
            {
                return await FarmManagementControllers.DeleteGeofence(repo, geofenceId);
            })
            .RequireAuthorization()
            .WithTags("Farm Geofencing");

        }
    }


}

