
using Domain.PremiseManagement.Requests;
using FluentValidation;

namespace Api.PremiseManagement.Validators
{
    public class PremiseCreationValidator : AbstractValidator<PremiseCreationRequest>
    {
        public PremiseCreationValidator()
        {
            RuleFor(premise => premise.Name)
                .NotEmpty().WithMessage("Premise name is required.")
                .Length(3, 50).WithMessage("Premise name must be between 3 and 50 characters.");

           
        }
    }
}
