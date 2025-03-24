
using Domain.PremiseManagement.Requests;
using FluentValidation;

namespace Api.PremiseManagement.Validators
{
    public class PremiseUpdateRequestValidator : AbstractValidator<PremiseUpdateRequest>
    {
        public PremiseUpdateRequestValidator()
        {
            RuleFor(Premise => Premise.Name)
                .NotEmpty().WithMessage("Premise name is required.")
                .Length(3, 50).WithMessage("Premise name must be between 3 and 50 characters.");

          
        }
    }
}
