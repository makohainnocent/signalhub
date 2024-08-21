using Domain.FarmManagement.Requests;
using FluentValidation;

namespace Api.FarmManagement.Validators
{
    public class FarmGeofencingValidator : AbstractValidator<FarmGeofencingRequest>
    {
        public FarmGeofencingValidator()
        {
            RuleFor(geofencing => geofencing.FarmId)
                .GreaterThan(0).WithMessage("Farm ID must be a positive integer.");

            RuleFor(geofencing => geofencing.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.");

            RuleFor(geofencing => geofencing.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.");

            RuleFor(geofencing => geofencing.Radius)
                .GreaterThan(0).WithMessage("Radius must be greater than 0 meters.");
        }
    }
}
