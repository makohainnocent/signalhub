using Api.Common.utilities;
using Application.Authentication.Abstractions;
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
                return Results.Ok(createdUser);

            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }


        public static async Task<IResult> LoginUser(IAuthenticationRepository repo, UserLoginRequest request, TokenService tokenService)
        {
            try
            {
                
                User user = await repo.LoginUser(request);

                if (user == null)
                {
                    return Results.BadRequest(new { message = "Invalid username or password." });
                }

                var token = await tokenService.GenerateTokenAsync(user.UserId);

                return Results.Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

    }
}
