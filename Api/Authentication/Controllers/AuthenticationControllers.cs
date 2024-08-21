using Application.Authentication.Abstractions;
using DataAccess.Authentication.Exceptions;
using DataAccess.Authentication.Utilities;
using Domain.Authentication.Requests;
using Domain.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Serilog;
using Api.Authentication.Utilities;
using Application.Common.Abstractions;
using System.Security.Claims;
using Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Authentication.Controllers
{
    public class AuthenticationControllers
    {
        public static async Task<IResult> CreateUser(IAuthenticationRepository repo, UserRegistrationRequest request)
        {
            try
            {
                Log.Information("Attempting to create user with username: {Username}", request.Username);

                User createdUser = await repo.CreateUser(request);

                Log.Information("User created successfully with ID: {UserId}", createdUser.UserId);
                return Results.Created($"/users/{createdUser.UserId}", createdUser);
            }
            catch (UserAlreadyExistsException ex)
            {
                Log.Warning(ex, "User creation failed - user already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating user.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> LoginUser(IAuthenticationRepository repo, UserLoginRequest request, TokenService tokenService)
        {
            try
            {
                Log.Information("Attempting to log in user with username: {Username}", request.UsernameOrEmail);

                User user = await repo.LoginUser(request);

                if (user == null)
                {
                    Log.Warning("Login failed - invalid credentials for username: {Username}", request.UsernameOrEmail);
                    return Results.Unauthorized();
                }

                var accessToken = await tokenService.GenerateTokenAsync(user.UserId);
                var refreshToken = await tokenService.GenerateRefreshTokenAsync(user.UserId);

                Log.Information("User logged in successfully with ID: {UserId}", user.UserId);
                return Results.Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
            }
            catch (InvalidCredentialsException ex)
            {
                Log.Warning(ex, "Login failed - invalid credentials.");
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during user login.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> RefreshToken(IAuthenticationRepository repo, TokenService tokenService, RefreshTokenRequest request)
        {
            try
            {
                Log.Information("Attempting to refresh token for refresh token: {RefreshToken}", request.RefreshToken);

                var refreshToken = await repo.GetByTokenAsync(request.RefreshToken);

                if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
                {
                    Log.Warning("Token refresh failed - invalid or expired refresh token.");
                    return Results.Json(new { message = "Invalid or expired refresh token." }, statusCode: StatusCodes.Status401Unauthorized);
                }

                await repo.RevokeAsync(request.RefreshToken);

                var user = await repo.GetUserByIdAsync(refreshToken.UserId);
                var newAccessToken = await tokenService.GenerateTokenAsync(user.UserId);
                var newRefreshToken = await tokenService.GenerateRefreshTokenAsync(user.UserId);

                Log.Information("Token refreshed successfully for user ID: {UserId}", user.UserId);
                return Results.Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while refreshing token.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> ForgotPassword(IAuthenticationRepository repo, Domain.Authentication.Requests.ForgotPasswordRequest request, IEmailService emailService)
        {
            try
            {
                Log.Information("Processing forgot password request for email: {Email}", request.Email);

                string verificationCode = VerificationCode.GenerateVerificationCode();

                await repo.StoreVerificationCode(request.Email, verificationCode);
                await VerificationCode.SendVerificationCodeAsync(emailService, request.Email, verificationCode);

                Log.Information("Verification code sent for password reset to email: {Email}", request.Email);
                return Results.Ok(new { message = "Verification code sent" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during the forgot password process.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> ResetPassword(IAuthenticationRepository repo, Domain.Authentication.Requests.ResetPasswordRequest request)
        {
            try
            {
                Log.Information("Attempting to reset password for email: {Email}", request.Email);

                
                bool isValidCode = await repo.ValidateVerificationCode(request.Email, request.Code);

                if (!isValidCode)
                {
                    Log.Warning("Invalid verification code for email: {Email}", request.Email);
                    return Results.Json(new { message = "Invalid verification code" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                
                await repo.UpdatePassword(request.Email, request.NewPassword);

                Log.Information("Password reset successfully for email: {Email}", request.Email);
                return Results.Ok(new { message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during the password reset process.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> ChangePassword(IAuthenticationRepository repo, Domain.Authentication.Requests.ChangePasswordRequest request)
        {
            try
            {
                Log.Information("Attempting to change password for email: {Email}", request.Email);

                
                bool isValidOldPassword = await repo.ValidatePassword(request.Email, request.OldPassword);

                if (!isValidOldPassword)
                {
                    Log.Warning("Invalid old password for email: {Email}", request.Email);
                    return Results.Json(new { message = "Invalid old password" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                
                await repo.UpdatePassword(request.Email, request.NewPassword);

                Log.Information("Password changed successfully for email: {Email}", request.Email);
                return Results.Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while changing password.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateUser(IAuthenticationRepository repo, [FromBody] UpdateUserRequest request)
        {
            try
            {
                
                if (request.UserId <= 0)
                {
                    return Results.BadRequest(new { message = "Valid UserId must be provided." });
                }

                
                var existingUser = await repo.GetUserByIdAsync(request.UserId);
                if (existingUser == null)
                {
                    return Results.NotFound(new { message = "User not found." });
                }

                await repo.UpdateUser(existingUser.UserId, request);

                return Results.Ok(new { message = "User details updated successfully." });
            }
            catch (UserAlreadyExistsException ex)
            {
                
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating user details.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetUserById(IAuthenticationRepository repo, [FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return Results.BadRequest(new { message = "Invalid user ID provided." });
            }

            try
            {
                var user = await repo.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return Results.NotFound(new { message = "User not found." });
                }

                return Results.Ok(user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving user details.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetUserByClaim(IAuthenticationRepository repo, ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Results.BadRequest(new { message = "User ID not found in claims." });
            }

            try
            {
                var existingUser = await repo.GetUserByIdAsync(userId);
                if (existingUser == null)
                {
                    return Results.NotFound(new { message = "User not found." });
                }

                return Results.Ok(existingUser);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving user details.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetUsersAll(IAuthenticationRepository repo, int pageNumber = 1, int pageSize = 10, string? search = null)
        {
            
            if (pageNumber <= 0)
            {
                return Results.BadRequest(new { message = "Page number must be greater than 0." });
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                return Results.BadRequest(new { message = "Page size must be between 1 and 100." });
            }

            try
            {
                
                var pagedResult = await repo.GetUsersAsync(pageNumber, pageSize, search);

                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving users.");
                return Results.Problem(ex.Message);
            }
        }


        public static async Task<IResult> DeleteUser(IAuthenticationRepository repo, int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return Results.BadRequest(new { message = "Invalid UserId." });
                }

                var user = await repo.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return Results.NotFound(new { message = "User not found." });
                }

                await repo.DeleteUserAsync(userId);
                return Results.Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the user.");
                return Results.Problem(ex.Message);
            }
        }


        public static async Task<IResult> AddRole(IAuthenticationRepository repo, RoleRequest roleRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleRequest.RoleName))
                {
                    return Results.BadRequest(new { message = "RoleName cannot be empty." });
                }

                var newRole = new Role
                {
                    RoleName = roleRequest.RoleName
                };

                var roleId = await repo.AddRoleAsync(newRole);
                return Results.Ok(new { message = "Role added successfully.", RoleId = roleId });
            }
            catch (RoleAlreadyExistsException ex)
            {

                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding the role.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateRole(IAuthenticationRepository repo, UpdateRoleRequest roleRequest)
        {
            try
            {
                if (roleRequest.RoleId <= 0)
                {
                    return Results.BadRequest(new { message = "Invalid RoleId." });
                }

                if (string.IsNullOrWhiteSpace(roleRequest.RoleName))
                {
                    return Results.BadRequest(new { message = "RoleName cannot be empty." });
                }

                var updatedRole = await repo.UpdateRoleAsync(roleRequest);

                if (updatedRole == null)
                {
                    return Results.NotFound(new { message = "Role not found." });
                }

                return Results.Ok(updatedRole);
            }
            catch (RoleAlreadyExistsException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the role.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetRolesAll(IAuthenticationRepository repo, int page = 1, int pageSize = 10, string? search = null)
        {
            try
            {
                
                if (page < 1 || pageSize < 1)
                {
                    return Results.BadRequest(new { message = "Page and PageSize must be greater than 0." });
                }

                
                var pagedResult = await repo.GetRolesAsync(page, pageSize, search);

                
                if (!pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No roles found." });
                }

                
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the roles.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> AddClaim(IAuthenticationRepository repo, AddClaimRequest request)
        {
            try
            {
               
                if (string.IsNullOrWhiteSpace(request.ClaimType) || string.IsNullOrWhiteSpace(request.ClaimValue))
                {
                    return Results.BadRequest(new { message = "ClaimType and ClaimValue are required." });
                }

               
                var claimId = await repo.AddClaimAsync(request);

                return Results.Created($"/claims/{claimId}", new { ClaimId = claimId });
            }
            catch (ItemAlreadyExistsException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding the claim.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateClaim(IAuthenticationRepository repo, UpdateClaimRequest request)
        {
            try
            {
                if (request.ClaimId <= 0 || string.IsNullOrWhiteSpace(request.ClaimType) || string.IsNullOrWhiteSpace(request.ClaimValue))
                {
                    return Results.BadRequest(new { message = "ClaimId, ClaimType, and ClaimValue are required." });
                }

               
                var updatedClaim = await repo.UpdateClaimAsync(request);

                if (updatedClaim == null)
                {
                    return Results.NotFound(new { message = "Claim not found." });
                }

                return Results.Ok(updatedClaim);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the claim.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetClaims(IAuthenticationRepository repo, int pageNumber = 1, int pageSize = 10, string? search = null)
        {
            try
            {
                
                if (pageNumber <= 0)
                {
                    return Results.BadRequest(new { message = "Page number must be greater than 0." });
                }

                if (pageSize <= 0 || pageSize > 100)
                {
                    return Results.BadRequest(new { message = "Page size must be between 1 and 100." });
                }

                
                var pagedResult = await repo.GetClaimsAsync(pageNumber, pageSize, search);

                if (!pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No claims found." });
                }

                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving claims.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetClaimById(IAuthenticationRepository repo, int claimId)
        {
            try
            {
                
                var claim = await repo.GetClaimByIdAsync(claimId);

                if (claim == null)
                {
                    return Results.NotFound(new { message = "Claim not found." });
                }

                return Results.Ok(claim);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the claim.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteClaimAsync(IAuthenticationRepository repo, int claimId)
        {
            try
            {
                await repo.DeleteClaimAsync(claimId);
                return Results.Ok(new { message = "Claim successfully deleted." });
            }
            catch (ItemDoesNotExistException ex)
            {
                return Results.NotFound(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the claim.");
                return Results.Problem(ex.Message); 
            }
        }

        public static async Task<IResult> DeleteRoleAsync(IAuthenticationRepository repo, int roleId)
        {
            try
            {
                await repo.DeleteRoleAsync(roleId);
                return Results.Ok(new { message = "Role successfully deleted." }); 
            }
            catch (ItemDoesNotExistException ex)
            {
                return Results.NotFound(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the role.");
                return Results.Problem(ex.Message); 
            }
        }

        public static async Task<IResult> GetRoleAsync(IAuthenticationRepository repo, int roleId)
        {
            try
            {
                var role = await repo.GetRoleAsync(roleId);

                if (role == null)
                {
                    return Results.NotFound(new { message = "Role not found." }); 
                }

                return Results.Ok(role); 
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the role.");
                return Results.Problem(ex.Message); 
            }
        }

        public static async Task<IResult> AddRoleToUserAsync(IAuthenticationRepository repo, AddRoleToUserRequest request)
        {
            try
            {
                bool result = await repo.AddRoleToUserAsync(request);

                if (result)
                {
                    return Results.Ok(new { message = "Role added to user successfully." });
                }
                else
                {
                    return Results.BadRequest("Failed to add role");
                }
                
            }
            catch (ItemAlreadyExistsException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (ItemDoesNotExistException ex)
            {
                return Results.NotFound(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding the role to the user.");
                return Results.Problem(ex.Message); 
            }
        }

        public static async Task<IResult> RemoveRoleFromUserAsync(IAuthenticationRepository repo, RemoveRoleFromUserRequest request)
        {
            try
            {
                bool result = await repo.RemoveRoleFromUserAsync(request);

                if (result)
                {
                    return Results.Ok(new { message = "Role removed from user successfully." });
                }
                else
                {
                    return Results.NotFound(new { message = "Role was not assigned to the user or does not exist." });
                }
            }
            catch (ItemDoesNotExistException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while removing the role from the user.");
                return Results.Problem(ex.Message); 
            }
        }

        public static async Task<IResult> GetRolesByUserAsync(IAuthenticationRepository repo, int userId)
        {
            try
            {
                
                var roles = await repo.GetRolesByUserIdAsync(userId);

               
                if (roles == null || !roles.Any())
                {
                    
                    return Results.NotFound(new { message = $"No roles found for user with UserId: {userId}" });
                }

                
                return Results.Ok(roles);
            }
            catch (ItemDoesNotExistException ex)
            {
                
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                
                Log.Error(ex, "An error occurred while retrieving roles for the user.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetUserRolesAsync(IAuthenticationRepository repo, int pageNumber, int pageSize, string? search)
        {
            try
            {
                var result = await repo.GetUserRolesAsync(pageNumber, pageSize, search);

                if (result.Items == null || !result.Items.Any())
                {
                    return Results.NotFound(new { message = "No user roles found." });
                }

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving user roles.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> AddClaimToRoleAsync(IAuthenticationRepository repo, AddClaimToRoleRequest request)
        {
            try
            {
                bool result = await repo.AddClaimToRoleAsync(request);

                if (result)
                {
                    return Results.Ok(new { message = "Claim added to role successfully." });
                }
                else
                {
                    return Results.BadRequest(new { message = "Failed to add claim to role." });
                }
            }
            catch (ItemAlreadyExistsException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (ItemDoesNotExistException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding the claim to the role.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> RemoveClaimFromRoleAsync(IAuthenticationRepository repo, RemoveClaimFromRoleRequest request)
        {
            try
            {
                bool result = await repo.RemoveClaimFromRoleAsync(request);

                if (result)
                {
                    return Results.Ok(new { message = "Claim removed from role successfully." });
                }
                else
                {
                    return Results.BadRequest(new { message = "Failed to remove claim from role." });
                }
            }
            catch (ItemDoesNotExistException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while removing the claim from the role.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetClaimsByRoleIdAsync(IAuthenticationRepository repo, int roleId)
        {
            try
            {
                var claims = await repo.GetClaimsByRoleIdAsync(roleId);

                if (claims.Any())
                {
                    return Results.Ok(claims);
                }
                else
                {
                    return Results.NotFound(new { message = "No claims found for the specified role." });
                }
            }
            catch (ItemDoesNotExistException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving claims for the role.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetRoleClaimsAsync(IAuthenticationRepository repo, int pageNumber, int pageSize, string? search = null)
        {
            try
            {
                var roleClaims = await repo.GetRoleClaimsAsync(pageNumber, pageSize, search);

                if (roleClaims.Items == null || !roleClaims.Items.Any())
                {
                    return Results.NotFound(new { message = "No role claims found." });
                }

                return Results.Ok(roleClaims);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving role claims.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> AddClaimToUserAsync(IAuthenticationRepository repo, AddClaimToUserRequest request)
        {
            try
            {
                bool result = await repo.AddClaimToUserAsync(request);

                if (result)
                {
                    return Results.Ok(new { message = "Claim added to user successfully." });
                }
                else
                {
                    return Results.BadRequest(new { message = "Failed to add claim to user." });
                }
            }
            catch (ItemAlreadyExistsException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (ItemDoesNotExistException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding the claim to the user.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> RemoveClaimFromUserAsync(IAuthenticationRepository repo, RemoveClaimFromUserRequest request)
        {
            try
            {
                bool result = await repo.RemoveClaimFromUserAsync(request);

                if (result)
                {
                    return Results.Ok(new { message = "Claim removed from user successfully." });
                }
                else
                {
                    return Results.BadRequest(new { message = "Failed to remove claim from user." });
                }
            }
            catch (ItemDoesNotExistException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while removing the claim from the user.");
                return Results.Problem(ex.Message);
            }
        }


        public static async Task<IResult> GetUserClaimsAsync(IAuthenticationRepository repo, int pageNumber, int pageSize, string? search = null)
        {
            try
            {
                var result = await repo.GetUserClaimsAsync(pageNumber, pageSize, search);
                if (result.Items == null || !result.Items.Any())
                {
                    return Results.NotFound(new { message = "No user claims found." });
                }

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving user claims.");
                return Results.Problem(ex.Message);
            }
        }



    }
}
