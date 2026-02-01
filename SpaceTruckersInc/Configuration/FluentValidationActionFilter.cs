using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SpaceTruckersInc.Configuration;

public sealed class FluentValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (KeyValuePair<string, object?> arg in context.ActionArguments)
        {
            if (arg.Value is null)
            {
                continue;
            }

            Type argType = arg.Value.GetType();
            Type validatorType = typeof(IValidator<>).MakeGenericType(argType);
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            ValidationContext<object> validationContext = new(arg.Value!);
            ValidationResult result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                foreach (ValidationFailure? failure in result.Errors)
                {
                    _ = context.ModelState.TryAddModelError(failure.PropertyName ?? string.Empty, failure.ErrorMessage);
                }

                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }
        }

        _ = await next();
    }
}