namespace TracesToCsv.Utilities;

public class ApiExceptionHandlingMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlingMiddleware> logger) : ILoggable<ApiExceptionHandlingMiddleware>
{
    ILogger<ApiExceptionHandlingMiddleware>? ILoggable<ApiExceptionHandlingMiddleware>.Logger => logger;

    public async Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            this.LogError("Error: " + ex);
            if (context.Response.HasStarted) // too late, no can do
                throw;

            var handled = await HandleApiExceptionAsync(context, ex);
            if (!handled)
                throw;
        }
    }

    protected virtual async ValueTask<bool> HandleApiExceptionAsync(HttpContext context, Exception ex)
    {
        if (ex is ArgumentException ae)
        {
            var details = new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [ae.ParamName ?? "error"] = [ae.GetInterestingExceptionMessage() ?? "Unknown error."]
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(JsonSerializer.Serialize(details));
            return true;
        }
        return false;
    }
}
