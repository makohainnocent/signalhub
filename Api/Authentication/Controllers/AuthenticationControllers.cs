using Serilog;
using Application.Authentication.Abstractions;
using DataAccess.Authentication.Exceptions;
using DataAccess.Authentication.Utilities;
using Domain.Authentication.Requests;
using Domain.Core.Models;
using Microsoft.AspNetCore.Mvc;

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
    }
}
