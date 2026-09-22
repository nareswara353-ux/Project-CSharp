using System.Diagnostics;

namespace Application.Common.Diagnostics;

public static class ActivityExtensions
{
    public static void RecordException(this Activity? activity, Exception exception)
    {
        if (activity is null)
            return;

        var tags = new ActivityTagsCollection
        {
            ["exception.type"] = exception.GetType().FullName,
            ["exception.message"] = exception.Message,
            ["exception.stacktrace"] = exception.StackTrace
        };

        activity.AddEvent(new ActivityEvent("exception", tags: tags));
    }
}
