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
                .NotEmpty().WithMessage("Username or Email is required.");
                

            // Validate Password
            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password is required.");
                
        }

       

       
    }
}
