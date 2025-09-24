using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace CarDealership.Api.Filters;

public class ResponseEnvelopeFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var value = objectResult.Value;

            // Skip ProblemDetails results (handled by model state or exceptions)
            if (value is ProblemDetails || value is ValidationProblemDetails)
            {
                await next();
                return;
            }

            // If the value already has a Success/success property, assume it's enveloped
            if (value != null && HasProperty(value, "Success"))
            {
                await next();
                return;
            }

            bool isSuccess = (objectResult.StatusCode ?? 200) >= 200 && (objectResult.StatusCode ?? 200) < 300;
            var payload = new
            {
                success = isSuccess,
                message = string.Empty,
                data = value ?? new { }
            };

            context.Result = new ObjectResult(payload)
            {
                StatusCode = objectResult.StatusCode ?? 200,
                DeclaredType = payload.GetType()
            };
        }
        else if (context.Result is NotFoundResult)
        {
            var payload = new { success = false, message = "Not found", data = new { } };
            context.Result = new NotFoundObjectResult(payload);
        }
        else if (context.Result is OkResult)
        {
            var payload = new { success = true, message = string.Empty, data = new { } };
            context.Result = new OkObjectResult(payload);
        }

        await next();
    }

    private static bool HasProperty(object instance, string name)
    {
        var type = instance.GetType();
        // Case-insensitive search for property name (handles anonymous objects and typed responses)
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}


