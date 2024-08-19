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
using Microsoft.AspNetCore.Identity.Data;
using ForgotPasswordRequest = Domain.Authentication.Requests.ForgotPasswordRequest;
using ResetPasswordRequest = Domain.Authentication.Requests.ResetPasswordRequest;
using Application.Common.Abstractions;
using System.Security.Claims;

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


                auth.MapPost("/login", async (IAuthenticationRepository repo, [FromBody] UserLoginRequest request, TokenService tokenService) =>
                {
                    return await AuthenticationControllers.LoginUser(repo, request, tokenService);
                })
                .AddEndpointFilter<ValidationFilter<UserLoginRequest>>()
                .AllowAnonymous();

                 auth.MapPost("/refresh-token", async (IAuthenticationRepository repo , TokenService tokenService, [FromBody] RefreshTokenRequest request) =>
                 {
                     return await AuthenticationControllers.RefreshToken(repo, tokenService, request);
                 }).AllowAnonymous();

            auth.MapPost("/forgot-password", async (IAuthenticationRepository repo, [FromBody] ForgotPasswordRequest request, IEmailService emailService) =>
            {
                return await AuthenticationControllers.ForgotPassword(repo, request, emailService);
            })
            .AddEndpointFilter<ValidationFilter<ForgotPasswordRequest>>()
            .AllowAnonymous();

            auth.MapPost("/reset-password", async (IAuthenticationRepository repo, [FromBody] ResetPasswordRequest request) =>
                {
                    return await AuthenticationControllers.ResetPassword(repo, request);
                })
                .AddEndpointFilter<ValidationFilter<ResetPasswordRequest>>()
                .AllowAnonymous();


            auth.MapPost("/change-password", async (IAuthenticationRepository repo, [FromBody] ChangePasswordRequest request, ClaimsPrincipal user) =>
            {
                
                var email = user.FindFirst(ClaimTypes.Email)?.Value;

                
                return await AuthenticationControllers.ChangePassword(repo, request);
            })
            .AddEndpointFilter<ValidationFilter<ChangePasswordRequest>>()
            .RequireAuthorization();

            auth.MapPost("/update-user", async (IAuthenticationRepository repo, ClaimsPrincipal user, [FromBody] UpdateUserRequest request) =>
            {
                return await AuthenticationControllers.UpdateUser(repo, request);
            })
            .AddEndpointFilter<ValidationFilter<UpdateUserRequest>>()
            .RequireAuthorization(); 


        }
    }
    
}

