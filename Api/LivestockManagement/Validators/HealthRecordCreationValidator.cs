namespace Api.LivestockManagement.Validators
{
    using Domain.LivestockManagement.Requests;
    using FluentValidation;
    using System;

    public class HealthRecordCreationValidator : AbstractValidator<HealthRecordCreationRequest>
    {
        public HealthRecordCreationValidator()
        {
            RuleFor(record => record.LivestockId)
                .GreaterThan(0).WithMessage("Livestock ID is required and must be a positive number.");

            RuleFor(record => record.UserId)
                .GreaterThan(0).WithMessage("User ID is required and must be a positive number.");

            RuleFor(record => record.DateOfVisit)
                .NotEmpty().WithMessage("Date of Visit is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date of Visit must be today or in the past.");

            RuleFor(record => record.Diagnosis)
                .NotEmpty().WithMessage("Diagnosis is required.")
                .Length(3, 200).WithMessage("Diagnosis must be between 3 and 200 characters.");

            RuleFor(record => record.Treatment)
                .NotEmpty().WithMessage("Treatment is required.")
                .Length(3, 200).WithMessage("Treatment must be between 3 and 200 characters.");

           

            
        }
    }

}
