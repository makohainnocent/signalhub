using Domain.FarmManagement.Requests;
using FluentValidation;

namespace Api.FarmManagement.Validators
{
    public class FarmCreationValidator : AbstractValidator<FarmCreationRequest>
    {
        public FarmCreationValidator()
        {
            RuleFor(farm => farm.FarmName)
                .NotEmpty().WithMessage("Farm name is required.")
                .Length(3, 50).WithMessage("Farm name must be between 3 and 50 characters.");

            RuleFor(farm => farm.Location)
                .NotEmpty().WithMessage("Location is required.")
                .Length(5, 100).WithMessage("Location must be between 5 and 100 characters.");

            RuleFor(farm => farm.Area)
                .GreaterThan(0).WithMessage("Area must be greater than 0 acres.");
        }
    }
}
