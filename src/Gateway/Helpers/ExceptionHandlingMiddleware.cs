using Microsoft.AspNetCore.Mvc;

namespace Gateway.Helpers;

public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;
    
    public ExceptionHandlingMiddleware(RequestDelegate next,  ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured at '{Route}'", context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var response = new ProblemDetails
            {
                Title = "Unhandled Exception",
                Detail = ex.Message,
                Status = 500,
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}