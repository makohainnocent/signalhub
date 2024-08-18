using Api.Common.utilities;
using Application.Authentication.Abstractions;
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

    }
}
