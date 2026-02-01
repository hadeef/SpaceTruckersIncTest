using FluentValidation;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Application.DTOs.Validators;

public sealed class RecordEventRequestValidator : AbstractValidator<RecordEventRequest>
{
    public RecordEventRequestValidator()
    {
        _ = RuleFor(r => r.TripId).NotEmpty();

        _ = RuleFor(r => r.EventType)
            .NotEmpty()
            .WithMessage("EventType is required.")
            .Must(name => TripEventType.GetNames()
                .Contains(name!.Trim(), StringComparer.OrdinalIgnoreCase))
            .When(r => !string.IsNullOrWhiteSpace(r.EventType))
            .WithMessage("Invalid event type.");

        _ = RuleFor(r => r.Details).MaximumLength(1000);
    }
}