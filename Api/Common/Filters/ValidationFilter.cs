using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Api.Common.Filters
{
    public class ValidationFilter<TRequest> : IEndpointFilter where TRequest : class
    {
        private readonly IValidator<TRequest> _validator;

        public ValidationFilter(IValidator<TRequest> validator)
        {
            _validator = validator;
            Console.WriteLine($"Validator of type {typeof(TRequest)} registered");
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var request = context.Arguments.SingleOrDefault(x => x?.GetType() == typeof(TRequest)) as TRequest;

            if (request == null)
            {
                return Results.BadRequest("Invalid request object.");
            }

            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }

            return await next(context);
        }
    }
}
