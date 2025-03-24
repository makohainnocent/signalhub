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
using Api.Core.Services;


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

            // User Registration
            auth.MapPost("/register", async (IAuthenticationRepository repo, [FromBody] UserRegistrationRequest request) =>
            {
                return await AuthenticationControllers.CreateUser(repo, request);
            })
            .AddEndpointFilter<ValidationFilter<UserRegistrationRequest>>()
            .AllowAnonymous();
        //.WithTags("User Management");

            // User Login
        auth.MapPost("/login", async (IAuthenticationRepository repo, [FromBody] UserLoginRequest request, TokenService tokenService, NotificationUtilityService notificationUtilityService) =>
        {
            return await AuthenticationControllers.LoginUser(repo, request, tokenService, notificationUtilityService);
        })
        .AddEndpointFilter<ValidationFilter<UserLoginRequest>>()
        .AllowAnonymous();
            //.WithTags("Authentication");

            // Refresh Token
            auth.MapPost("/refresh-token", async (IAuthenticationRepository repo, TokenService tokenService, [FromBody] RefreshTokenRequest request) =>
            {
                return await AuthenticationControllers.RefreshToken(repo, tokenService, request);
            })
            .AllowAnonymous();
            //.WithTags("Authentication");

         
            // Update User
            auth.MapPost("/update-user", async (IAuthenticationRepository repo, ClaimsPrincipal user, [FromBody] UpdateUserRequest request) =>
            {
                return await AuthenticationControllers.UpdateUser(repo, request);
            })
            .AddEndpointFilter<ValidationFilter<UpdateUserRequest>>();
            //.RequireAuthorization("UserManagement")
            //.WithTags("User Management");

            // Get User By Id
            auth.MapGet("/get-user-by-id", async (IAuthenticationRepository repo, [FromQuery] int userId) =>
            {
                return await AuthenticationControllers.GetUserById(repo, userId);
            });
            //.RequireAuthorization("UserManagement")
            //.WithTags("User Management");

            // Get User By Claims
            auth.MapGet("/get-user-by-claims", async (IAuthenticationRepository repo, ClaimsPrincipal user) =>
            {
                return await AuthenticationControllers.GetUserByClaim(repo, user);
            });
            //.RequireAuthorization("UserManagement")
            //.WithTags("User Management");

            // Get All Users
            auth.MapGet("/get-users-all", async (IAuthenticationRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null) =>
            {
                return await AuthenticationControllers.GetUsersAll(repo, pageNumber, pageSize, search);
            });
            //.RequireAuthorization("UserManagement")
            //.WithTags("User Management");

            auth.MapGet("/users-count-all", async (IAuthenticationRepository repo) =>
            {
                return await AuthenticationControllers.CountUsers(repo);
            });

            // Delete User
            auth.MapDelete("/delete-user/{userId:int}", async (IAuthenticationRepository repo, int userId, HttpContext httpContext) =>
            {
                var user = httpContext.User;
                var isAdmin = user.IsInRole("Admin");
                var permissions = user.Claims.FirstOrDefault(c => c.Type == "Permission")?.Value;
                return await AuthenticationControllers.DeleteUser(repo, userId);
            });
            //.RequireAuthorization("AdminOnly")
            //.WithTags("User Management");

            // Role Management
            auth.MapPost("/add-role", async (IAuthenticationRepository repo, RoleRequest roleRequest) =>
            {
                return await AuthenticationControllers.AddRole(repo, roleRequest);
            });
            //.RequireAuthorization("RoleManagement")
            //.WithTags("Role Management");

            auth.MapPut("/update-role", async (IAuthenticationRepository repo, UpdateRoleRequest roleRequest) =>
            {
                return await AuthenticationControllers.UpdateRole(repo, roleRequest);
            });
            //.RequireAuthorization("RoleManagement")
            //.WithTags("Role Management");

            auth.MapGet("/get-roles-all", async (IAuthenticationRepository repo, int page = 1, int pageSize = 10, string? search = null) =>
            {
                return await AuthenticationControllers.GetRolesAll(repo, page, pageSize, search);
            });
            //.RequireAuthorization("RoleManagement")
            //.WithTags("Role Management");

            // Claim Management
            auth.MapPost("/add-claim", async (IAuthenticationRepository repo, AddClaimRequest request) =>
            {
                return await AuthenticationControllers.AddClaim(repo, request);
            });
            //.RequireAuthorization("ClaimManagement")
            //.WithTags("Claim Management");

            auth.MapPut("/update-claim", async (IAuthenticationRepository repo, UpdateClaimRequest request) =>
            {
                return await AuthenticationControllers.UpdateClaim(repo, request);
            });
            //.RequireAuthorization("ClaimManagement")
            //.WithTags("Claim Management");

            auth.MapGet("/get-claims-all", async (IAuthenticationRepository repo, int pageNumber = 1, int pageSize = 10, string? search = null) =>
            {
                return await AuthenticationControllers.GetClaims(repo, pageNumber, pageSize, search);
            });
            //.RequireAuthorization("ClaimManagement")
            //.WithTags("Claim Management");

            auth.MapGet("/get-claim/{claimId}", async (IAuthenticationRepository repo, int claimId) =>
            {
                return await AuthenticationControllers.GetClaimById(repo, claimId);
            });
            //.RequireAuthorization("ClaimManagement")
            //.WithTags("Claim Management");

            auth.MapDelete("/delete-claim/{claimId:int}", async (IAuthenticationRepository repo, int claimId) =>
            {
                return await AuthenticationControllers.DeleteClaimAsync(repo, claimId);
            });
            //.RequireAuthorization("ClaimManagement")
            //.WithTags("Claim Management");

            auth.MapDelete("/delete-role/{roleId:int}", async (IAuthenticationRepository repo, int roleId) =>
            {
                return await AuthenticationControllers.DeleteRoleAsync(repo, roleId);
            });
            //.RequireAuthorization("RoleManagement")
            //.WithTags("Role Management");

            auth.MapGet("/get-role/{roleId:int}", async (IAuthenticationRepository repo, int roleId) =>
            {
                return await AuthenticationControllers.GetRoleAsync(repo, roleId);
            });
            //.RequireAuthorization("RoleManagement")
            //.WithTags("Role Management");

            // User Role Management
            auth.MapPost("/add-role-to-user", async (IAuthenticationRepository repo, AddRoleToUserRequest request) =>
            {
                return await AuthenticationControllers.AddRoleToUserAsync(repo, request);
            });
            //.RequireAuthorization("UserRoleManagement")
            //.WithTags("User Role Management");

            auth.MapPost("/remove-role-from-user", async (IAuthenticationRepository repo, RemoveRoleFromUserRequest request) =>
            {
                return await AuthenticationControllers.RemoveRoleFromUserAsync(repo, request);
            });
            //.RequireAuthorization("UserRoleManagement")
            //.WithTags("User Role Management");

            auth.MapGet("/user-roles", async (IAuthenticationRepository repo, [FromQuery] int userId) =>
            {
                return await AuthenticationControllers.GetRolesByUserAsync(repo, userId);
            });
            //.RequireAuthorization("UserRoleManagement")
            //.WithTags("User Role Management");

            auth.MapGet("/user-roles-all", async (IAuthenticationRepository repo, int pageNumber = 1, int pageSize = 10, string? search = null) =>
            {
                return await AuthenticationControllers.GetUserRolesAsync(repo, pageNumber, pageSize, search);
            });
            //.RequireAuthorization("UserRoleManagement")
            //.WithTags("User Role Management");

            auth.MapGet("/user-roles-pending", async (IAuthenticationRepository repo) =>
            {
                return await AuthenticationControllers.CountPendingUserRoles(repo);
            });

            // Role Claim Management
            auth.MapPost("/add-claim-to-role", async (IAuthenticationRepository repo, AddClaimToRoleRequest request) =>
            {
                return await AuthenticationControllers.AddClaimToRoleAsync(repo, request);
            });
            //.RequireAuthorization("RoleClaimManagement")
            //.WithTags("Role Claim Management");

            auth.MapPost("/remove-claim-from-role", async (IAuthenticationRepository repo, RemoveClaimFromRoleRequest request) =>
            {
                return await AuthenticationControllers.RemoveClaimFromRoleAsync(repo, request);
            });
            //.RequireAuthorization("RoleClaimManagement")
            //.WithTags("Role Claim Management");

            auth.MapGet("/role-claims", async (IAuthenticationRepository repo, [FromQuery] int roleId) =>
            {
                return await AuthenticationControllers.GetClaimsByRoleIdAsync(repo, roleId);
            });
            //.RequireAuthorization("RoleClaimManagement")
            //.WithTags("Role Claim Management");

            auth.MapGet("/role-claims-all", async (IAuthenticationRepository repo, int pageNumber = 1, int pageSize = 10, string? search = null) =>
            {
                return await AuthenticationControllers.GetRoleClaimsAsync(repo, pageNumber, pageSize, search);
            });
            //.RequireAuthorization("RoleClaimManagement")
            //.WithTags("Role Claim Management");

            auth.MapPost("/request-password-reset", async (IAuthenticationRepository repo, IEmailService emailService, [FromBody] ForgotPasswordRequest request) =>
            {
                try
                {
                    // Generate a reset token
                    var token = await repo.GeneratePasswordResetTokenAsync(request.Email);

                    // Send the token to the user's email
                    await emailService.SendEmailAsync(request.Email, "Password Reset", $"Your password reset token is: {token}");

                    return Results.Ok(new { message = "Password reset token sent to your email." });
                }
                catch (UserNotFoundException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while generating the password reset token.");
                    return Results.Problem(ex.Message);
                }
            })
            .AllowAnonymous();


            auth.MapPost("/reset-password", async (IAuthenticationRepository repo, [FromBody] ResetPasswordRequest request) =>
            {
                try
                {
                    bool result = await repo.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

                    if (result)
                    {
                        return Results.Ok(new { message = "Password reset successfully." });
                    }
                    else
                    {
                        return Results.BadRequest(new { message = "Failed to reset password." });
                    }
                }
                catch (UserNotFoundException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (InvalidTokenException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while resetting the password.");
                    return Results.Problem(ex.Message);
                }
            })
            .AllowAnonymous();

            auth.MapGet("/change-user-role-staus", async (IAuthenticationRepository repo, int userId, int roleId, string status) =>
            {
                return await AuthenticationControllers.UpdateUserRoleStatusAsync(repo, userId, roleId, status);
            });




        }
}

    
}

