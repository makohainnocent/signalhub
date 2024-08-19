namespace Api.Authentication.Validators
{
    using FluentValidation;
    using Domain.Authentication.Requests;

    namespace Api.Authentication.Validators
    {
        public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
        {
            public ChangePasswordRequestValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("A valid email address is required.");

                RuleFor(x => x.OldPassword)
                    .NotEmpty().WithMessage("Old password is required.");

                RuleFor(x => x.NewPassword)
                    .NotEmpty().WithMessage("New password is required.")
                    .MinimumLength(8).WithMessage("New password must be at least 8 characters long.")
                    .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
                    .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter.")
                    .Matches(@"[0-9]").WithMessage("New password must contain at least one digit.")
                    .Matches(@"[\W]").WithMessage("New password must contain at least one special character.");
            }
        }
    }

}
