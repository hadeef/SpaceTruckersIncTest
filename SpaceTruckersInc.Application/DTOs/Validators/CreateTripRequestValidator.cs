using FluentValidation;
using SpaceTruckersInc.Application.DTOs.Requests;

namespace SpaceTruckersInc.Application.DTOs.Validators;

public sealed class CreateTripRequestValidator : AbstractValidator<CreateTripRequest>
{
    public CreateTripRequestValidator()
    {
        RuleFor(r => r.DriverId).NotEmpty();
        RuleFor(r => r.VehicleId).NotEmpty();
        RuleFor(r => r.RouteId).NotEmpty();
    }
}