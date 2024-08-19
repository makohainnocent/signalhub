using Api.Common.utilities;
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
                
                User createdUser = await repo.CreateUser(request);

               
                return Results.Created($"/users/{createdUser.UserId}", createdUser);
            }
            catch (UserAlreadyExistsException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                
                return Results.Problem(ex.Message);
            }
        }



        public static async Task<IResult> LoginUser(IAuthenticationRepository repo, UserLoginRequest request, TokenService tokenService)
        {
            try
            {
                
                User user = await repo.LoginUser(request);

                if (user == null)
                {
                    
                    return Results.Unauthorized();
                }

                
                var accessToken = await tokenService.GenerateTokenAsync(user.UserId);
                var refreshToken = await tokenService.GenerateRefreshTokenAsync(user.UserId);

                
                return Results.Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
            }
            catch (InvalidCredentialsException ex)
            {
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
            
            catch (Exception ex)
            {
                
                return Results.Problem(ex.Message);
            }
        }



        public static async Task<IResult> RefreshToken(IAuthenticationRepository repo, TokenService tokenService,RefreshTokenRequest request)
        {
            try
            {

                var refreshToken = await repo.GetByTokenAsync(request.RefreshToken);

                if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
                {   
                    return Results.Json(new { message = "Invalid or expired refresh token." }, statusCode: StatusCodes.Status401Unauthorized);
                }


                await repo.RevokeAsync(request.RefreshToken);

                var user = await repo.GetUserByIdAsync(refreshToken.UserId);
                var newAccessToken = await tokenService.GenerateTokenAsync(user.UserId);
                var newRefreshToken = await tokenService.GenerateRefreshTokenAsync(user.UserId);

                return Results.Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });


            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

    }
}
