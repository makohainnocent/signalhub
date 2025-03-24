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
using Api.FarmManagement.Controllers;
using System.Net;
using static Api.FarmManagement.Controllers.PremiseManagementControllers;
using Application.PremiseManagement.Abstractions;
using Domain.PremiseManagement.Requests;


namespace Api.FarmManagement.EndPointDefinations
{

    public class PremiseManagementEndPoints : IEndpointDefinition
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

            var Premise = versionedGroup.MapGroup("/Premise");


            Premise.MapPost("/premises", async (IPremiseManagementRepository repo, [FromBody] PremiseCreationRequest request, HttpContext httpContext) =>
            {
                
                return await PremiseManagementControllers.CreatePremise(repo, request, httpContext);
            })
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<PremiseCreationRequest>>()
            .WithTags("Premise Management");

            Premise.MapGet("/premises-all", async (IPremiseManagementRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery]  int ? ownerId = null,[FromQuery] string? search = null ) =>
            {
                return await PremiseManagementControllers.GetAllPremisesAsync(repo, pageNumber, pageSize, search);
            })
            //.RequireAuthorization()
            .WithTags("Premise Management");

            Premise.MapGet("/premises/{PremisesId}", async (IPremiseManagementRepository repo, int PremisesId) =>
            {
                return await PremiseManagementControllers.GetPremiseByIdAsync(repo, PremisesId);
            })
            .RequireAuthorization()
            .WithTags("Premise Management");

           
             Premise.MapGet("/premises/userId/{userId}", async (IPremiseManagementRepository repo, int userId,[FromQuery] string? agent="no") =>
             {
                 return await PremiseManagementControllers.GetAllPremisesByUserIdAsync(repo, userId, agent);
             })
            //.RequireAuthorization()
            .WithTags("Premise Management");

            Premise.MapPut("/premises", async (IPremiseManagementRepository repo, [FromBody] PremiseUpdateRequest request) =>
            {
                return await PremiseManagementControllers.UpdatePremise(repo, request);
            })
             .RequireAuthorization()
             .WithTags("Premise Management");

            Premise.MapDelete("/premises/{PremisesId}", async (IPremiseManagementRepository repo, int PremisesId) =>
            {
                return await PremiseManagementControllers.DeletePremise(repo, PremisesId);
            })
            .RequireAuthorization()
            .WithTags("Premise Management");

            Premise.MapGet("/premises/count-premises", async (IPremiseManagementRepository repo) =>
            {
                return await PremiseManagementControllers.CountPremises(repo);
            })
            .WithTags("Premise Management");

          

         

        }
    }


}

