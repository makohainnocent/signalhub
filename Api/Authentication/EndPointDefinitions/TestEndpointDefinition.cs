using Api.Common.Abstractions;



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
