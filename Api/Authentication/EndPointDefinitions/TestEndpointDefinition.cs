using Api.Common.Abstractions;
using Application.Abstractions;
using Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Authentication.EndPointDefinitions
{
    public class TestEndpointDefinition : IEndpointDefinition
    {

        public void RegisterEndpoints(WebApplication app)

        {
            var auth = app.MapGroup("/api/auth");

            auth.MapGet("/register", () =>
            {

                var responseMessage = new { Message = "Test endpoint is working!" };

                return responseMessage;
            }).WithName("GetTest")
            .WithOpenApi(); ;


        }


    }
}
