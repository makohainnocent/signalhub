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

            auth.MapGet("/get-user-by-id", async (IAuthenticationRepository repo, [FromQuery] int userId) =>
            {
                return await AuthenticationControllers.GetUserById(repo, userId);
            })
            .RequireAuthorization();

            auth.MapGet("/get-user-by-claims", async (IAuthenticationRepository repo, ClaimsPrincipal user) =>
            {
                return await AuthenticationControllers.GetUserByClaim(repo,user);
            })
            .RequireAuthorization();

            auth.MapGet("/get-users", async (IAuthenticationRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10) =>
            {
                return await AuthenticationControllers.GetUsers(repo, pageNumber, pageSize);
            })
            .RequireAuthorization();

            auth.MapDelete("/delete-user/{userId:int}", async (IAuthenticationRepository repo, int userId, HttpContext httpContext) =>
            {
                var user = httpContext.User;
                var isAdmin = user.IsInRole("Admin");
                var permissions = user.Claims.FirstOrDefault(c => c.Type == "Permission")?.Value;
                return await AuthenticationControllers.DeleteUser(repo, userId);
            })
            .RequireAuthorization("AdminOnly");


        }
    }
    
}

