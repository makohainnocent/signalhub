using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Api.Common.Filters
{
    /*public class CandidateValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var candidate = context.GetArgument<Candidate>(1);
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(candidate, null, null);

            if (!Validator.TryValidateObject(candidate, validationContext, validationResults, true))
            {
                var errors = new Dictionary<string, string[]>();

                foreach (var validationResult in validationResults)
                {
                    var memberNames = validationResult.MemberNames;
                    foreach (var memberName in memberNames)
                    {
                        if (!errors.ContainsKey(memberName))
                        {
                            errors[memberName] = new string[] { validationResult.ErrorMessage };
                        }
                        else
                        {
                            var errorList = new List<string>(errors[memberName]);
                            errorList.Add(validationResult.ErrorMessage);
                            errors[memberName] = errorList.ToArray();
                        }
                    }
                }

                return new BadRequestObjectResult(new ValidationProblemDetails(errors));
            }

            return await next(context);
        }
    }*/
}
