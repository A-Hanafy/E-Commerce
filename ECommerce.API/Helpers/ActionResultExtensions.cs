using ECommerce.Core.Errors;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Helpers;

public static class ActionResultExtensions
{
    public static NotFoundObjectResult ToNotFoundResult(this ControllerBase controller, string? message = null)
    {
        return controller.NotFound(new ApiResponse(404, message));
    }

    public static BadRequestObjectResult ToBadRequestResult(this ControllerBase controller, string? message = null)
    {
        return controller.BadRequest(new ApiResponse(400, message));
    }
}