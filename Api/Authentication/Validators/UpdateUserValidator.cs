using Domain.Authentication.Requests;
using FluentValidation;

namespace Api.Authentication.Validators
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserValidator()
        {
            
            RuleFor(request => request)
                .Must(HaveAtLeastOneField).WithMessage("At least one field (Username, Email, FullName, Address) must be provided for update.");

            
            RuleFor(request => request.Username)
                .NotEmpty().WithMessage("Username is required.")
                .Length(5, 20).WithMessage("Username must be between 5 and 20 characters.")
                .When(request => !string.IsNullOrEmpty(request.Username));

            
            RuleFor(request => request.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .When(request => !string.IsNullOrEmpty(request.Email));

            
            RuleFor(request => request.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(50).WithMessage("Full name must be 50 characters or less.")
                .When(request => !string.IsNullOrEmpty(request.FullName));

            
            RuleFor(request => request.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(100).WithMessage("Address must be 100 characters or less.")
                .When(request => !string.IsNullOrEmpty(request.Address));
        }

        private bool HaveAtLeastOneField(UpdateUserRequest request)
        {
            return !string.IsNullOrEmpty(request.Username) ||
                   !string.IsNullOrEmpty(request.Email) ||
                   !string.IsNullOrEmpty(request.FullName) ||
                   !string.IsNullOrEmpty(request.Address);
        }
    }
}
