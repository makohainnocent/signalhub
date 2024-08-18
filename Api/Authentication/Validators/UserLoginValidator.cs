using Domain.Authentication.Requests;
using FluentValidation;

namespace Api.Authentication.Validators
{
    public class UserLoginValidator : AbstractValidator<UserLoginRequest>
    {
        public UserLoginValidator()
        {
            // Validate UsernameOrEmail
            RuleFor(user => user.UsernameOrEmail)
                .NotEmpty().WithMessage("Username or Email is required.")
                .Must(IsValidUsernameOrEmail).WithMessage("Invalid Username or Email format.");

            // Validate Password
            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password is required.");
                
        }

        // Custom validator to check if the input is a valid Username or Email
        private bool IsValidUsernameOrEmail(string usernameOrEmail)
        {
            return IsEmail(usernameOrEmail) || IsUsername(usernameOrEmail);
        }

        // Check if the input is a valid email
        private bool IsEmail(string email)
        {
            return email.Contains('@'); // Basic check for email, you can enhance this
        }

        // Check if the input is a valid username (no spaces and meets length requirements)
        private bool IsUsername(string username)
        {
            return !username.Contains(' ') && username.Length >= 5 && username.Length <= 20;
        }
    }
}
