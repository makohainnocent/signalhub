using Api.Abstractions;
using Application.Abstractions;
using Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.EndPointDefinitions
{
    public class TestEndpointDefinition : IEndpointDefinition
    {
        
        public void RegisterEndpoints(WebApplication app)

        {
            app.MapGet("/api/test", () =>
            {
               
                var responseMessage = new { Message = "Test endpoint is working!" };

                return responseMessage;
            }).WithName("GetTest")
            .WithOpenApi(); ;

           
        }

       
    }
}
