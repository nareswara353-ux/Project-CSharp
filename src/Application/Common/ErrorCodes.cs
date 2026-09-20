namespace Application.Common;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string CreateFailed = "CREATE_FAILED";
    public const string UpdateFailed = "UPDATE_FAILED";
    public const string DeleteFailed = "DELETE_FAILED";
    public const string ConfirmFailed = "CONFIRM_FAILED";
    public const string ShipFailed = "SHIP_FAILED";
    public const string CancelFailed = "CANCEL_FAILED";
    public const string AddLineFailed = "ADD_LINE_FAILED";
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string UserInactive = "USER_INACTIVE";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string UsernameTaken = "USERNAME_TAKEN";
    public const string EmailTaken = "EMAIL_TAKEN";
    public const string CustomerNotFound = "CUSTOMER_NOT_FOUND";
    public const string InvalidRole = "INVALID_ROLE";
    public const string InvalidCurrentPassword = "INVALID_CURRENT_PASSWORD";
    public const string SamePassword = "SAME_PASSWORD";
    public const string ChangePasswordFailed = "CHANGE_PASSWORD_FAILED";
    public const string UpdateRoleFailed = "UPDATE_ROLE_FAILED";
    public const string RegisterFailed = "REGISTER_FAILED";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string ActivateFailed = "ACTIVATE_FAILED";
    public const string GetAllFailed = "GET_ALL_FAILED";
    public const string GetOrdersFailed = "GET_ORDERS_FAILED";
    public const string IdMismatch = "ID_MISMATCH";
    public const string InvalidToken = "INVALID_TOKEN";
}
