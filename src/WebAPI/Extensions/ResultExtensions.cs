using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess is not null
                ? onSuccess(result.Value!)
                : new OkObjectResult(result.Value);
        }

        return FailureToActionResult(result.ErrorCode, result.Error);
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return FailureToActionResult(result.ErrorCode, result.Error);
    }

    public static IActionResult ToCreatedResult<T>(
        this Result<T> result,
        string actionName,
        object routeValues)
    {
        if (result.IsSuccess)
            return new CreatedAtActionResult(actionName, null, routeValues, result.Value);

        return FailureToActionResult(result.ErrorCode, result.Error);
    }

    private static IActionResult FailureToActionResult(string? errorCode, string? error)
    {
        var body = new { error, code = errorCode };

        return errorCode switch
        {
            ErrorCodes.NotFound => new NotFoundObjectResult(body),
            ErrorCodes.CustomerNotFound => new NotFoundObjectResult(body),
            ErrorCodes.UserNotFound => new NotFoundObjectResult(body),
            ErrorCodes.InvalidCredentials => new UnauthorizedObjectResult(body),
            ErrorCodes.UserInactive => new UnauthorizedObjectResult(body),
            ErrorCodes.InvalidToken => new UnauthorizedObjectResult(body),
            ErrorCodes.UsernameTaken => new ConflictObjectResult(body),
            ErrorCodes.EmailTaken => new ConflictObjectResult(body),
            ErrorCodes.BusinessRuleViolation => new ConflictObjectResult(body),
            ErrorCodes.IdMismatch => new BadRequestObjectResult(body),
            _ => new BadRequestObjectResult(body)
        };
    }
}
