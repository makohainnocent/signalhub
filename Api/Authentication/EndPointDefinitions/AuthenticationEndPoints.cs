using Api.Common.Abstractions;
using Api.Authentication.Controllers;
using Application.Authentication.Abstractions;
using Domain.Authentication.Requests;
using Api.Common.Filters;
using Api.Authentication.Validators;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Asp.Versioning.Builder;
using Asp.Versioning;
using DataAccess.Authentication.Utilities;

namespace Api.Authentication.EndPointDefinitions
{
   
        public class AuthenticationEndPoints : IEndpointDefinition
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
                     
                var auth = versionedGroup.MapGroup("/auth");

                auth.MapPost("/register", async (IAuthenticationRepository repo, [FromBody] UserRegistrationRequest request) =>
                {
                    return await AuthenticationControllers.CreateUser(repo, request);
                })
                .AddEndpointFilter<ValidationFilter<UserRegistrationRequest>>()
                .AllowAnonymous();


                app.MapPost("/login", async (IAuthenticationRepository repo, [FromBody] UserLoginRequest request, TokenService tokenService) =>
                {
                    return await AuthenticationControllers.LoginUser(repo, request, tokenService);
                })
                .AddEndpointFilter<ValidationFilter<UserLoginRequest>>()
                .AllowAnonymous();
        }
        }
    
}

