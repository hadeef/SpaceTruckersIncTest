using Microsoft.AspNetCore.Mvc;
using SpaceTruckersInc.Application.Common;

namespace SpaceTruckersInc.API;

public static class ServiceResponseExtensions
{
    /// <summary>
    /// Convert a ServiceResponse into an IActionResult using the controller helpers.
    /// Optionally provide an onSuccess callback for 200/201 responses that need custom results
    /// (e.g., CreatedAtAction).
    /// </summary>
    public static IActionResult ToActionResult<T>(this ServiceResponse<T> response, ControllerBase controller
        , Func<T, IActionResult>? onSuccess = null)
    {
        if (response is null) throw new ArgumentNullException(nameof(response));
        if (controller is null) throw new ArgumentNullException(nameof(controller));

        return response.StatusCode switch
        {
            StatusCodes.Status200OK when response.Data is not null
                => onSuccess?.Invoke(response.Data) ?? controller.Ok(response.Data),

            StatusCodes.Status201Created when response.Data is not null
                => onSuccess?.Invoke(response.Data) ?? controller.Ok(response.Data),

            StatusCodes.Status204NoContent => controller.NoContent(),

            StatusCodes.Status404NotFound => controller.NotFound(new { errors = response.Errors, message = response.Message }),

            StatusCodes.Status400BadRequest => controller.BadRequest(new { errors = response.Errors, message = response.Message }),

            StatusCodes.Status409Conflict => controller.Conflict(new { errors = response.Errors, message = response.Message }),

            StatusCodes.Status500InternalServerError => controller.Problem(response.Message),

            _ => controller.Problem("An unexpected error occurred.")
        };
    }
}