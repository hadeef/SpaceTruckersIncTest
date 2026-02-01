using FluentValidation;
using SpaceTruckersInc.Application.DTOs.Requests;

namespace SpaceTruckersInc.Application.DTOs.Validators;

public sealed class CreateRouteRequestValidator : AbstractValidator<CreateRouteRequest>
{
    public CreateRouteRequestValidator()
    {
        RuleFor(r => r.Origin).NotEmpty().MaximumLength(200);
        RuleFor(r => r.Destination).NotEmpty().MaximumLength(200);
        RuleFor(r => r.EstimatedDuration).Must(d => d.TotalSeconds >= 0).WithMessage("EstimatedDuration must be non-negative.");
    }
}