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

        public static async Task<IResult> GetUsers(IAuthenticationRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // Validate query parameters
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
                // Fetch paged users from the repository
                var pagedResult = await repo.GetUsersAsync(pageNumber, pageSize);

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




    }
}
