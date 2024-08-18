using Api.Common.Abstractions;
using Api.Authentication.Controllers;
using Application.Authentication.Abstractions;
using Domain.Authentication.Requests;
using Api.Common.Filters;
using Api.Authentication.Validators;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace Api.Authentication.EndPointDefinitions
{
   
        public class AuthenticationEndPoints : IEndpointDefinition
        {
            public void RegisterEndpoints(WebApplication app)
            {
                var auth = app.MapGroup("/api/auth");

                auth.MapPost("/register", async (IAuthenticationRepository repo, [FromBody] UserRegistrationRequest request) =>
                {
                    return await AuthenticationControllers.CreateUser(repo, request);
                })
                .AddEndpointFilter<ValidationFilter<UserRegistrationRequest>>();
            }
        }
    
}

