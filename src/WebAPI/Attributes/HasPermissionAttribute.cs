using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public HasPermissionAttribute(string permission)
    {
        Policy = $"{PolicyPrefix}{permission}";
    }
}
