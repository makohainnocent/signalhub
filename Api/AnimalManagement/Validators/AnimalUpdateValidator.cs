
using Domain.AnimalManagement.Requests;
using FluentValidation;

namespace Api.AnimalManagement.Validators
{
    public class AnimalUpdateValidator : AbstractValidator<AnimalUpdateRequest>
    {
        public AnimalUpdateValidator()
        {
            RuleFor(livestock => livestock.Species)
                .NotEmpty().WithMessage("Species is required.")
                .Length(3, 50).WithMessage("Species name must be between 3 and 50 characters.");

            RuleFor(livestock => livestock.Breed)
                .NotEmpty().WithMessage("Breed is required.")
                .Length(3, 50).WithMessage("Breed name must be between 3 and 50 characters.");
            RuleFor(livestock => livestock.HealthStatus)
                .NotEmpty().WithMessage("Health Status is required.")
                .Length(3, 100).WithMessage("Health Status must be between 3 and 100 characters.");

            RuleFor(livestock => livestock.IdentificationMark)
                .NotEmpty().WithMessage("Identification Mark is required.")
                .Length(5, 50).WithMessage("Identification Mark must be between 5 and 50 characters.");
        }
    }
}
