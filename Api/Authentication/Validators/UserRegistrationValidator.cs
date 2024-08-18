
using Domain.Authentication.Requests;
using FluentValidation;

namespace Api.Authentication.Validators
{
    public class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
    {
        public UserRegistrationValidator()
        {
            RuleFor(user => user.Username)
                .NotEmpty().WithMessage("Username is required.")
                .Length(5, 20).WithMessage("Username must be between 5 and 20 characters.");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(user => user.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}
