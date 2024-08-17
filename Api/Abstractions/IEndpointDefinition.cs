namespace Api.Abstractions
{
    public interface IEndpointDefinition
    {
        void RegisterEndpoints(WebApplication app);
    }
}
