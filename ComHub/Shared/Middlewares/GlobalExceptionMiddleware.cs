using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Net;
using System.Text;
using System.Text.Json;
using ComHub.Shared.Exceptions;
using ComHub.Shared.Models;

namespace ComHub.Shared.Middlewares;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, string[]? apiPaths = null)
{
    private readonly RequestDelegate _next = next;
    private readonly string[] _apiPaths = apiPaths ?? ["/api"];

    public async Task Invoke(HttpContext context)
    {
        if (_apiPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
        {
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            try
            {
                await _next(context);

                responseBody.Seek(0, SeekOrigin.Begin);

                // Read the response body
                var text = await new StreamReader(responseBody).ReadToEndAsync();

                if (
                    string.IsNullOrWhiteSpace(text)
                    || context.Response.ContentType?.Contains("application/json") == true
                )
                {
                    // Transform the response
                    var transformedResponse = TransformResponse(context, text);

                    // Convert transformed response to JSON
                    var transformedJson = JsonSerializer.Serialize(transformedResponse);

                    // Write the transformed response directly to the original stream
                    context.Response.Body = originalBodyStream;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsync((string)transformedJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                context.Response.Body = originalBodyStream;

                await HandleExceptionAsync(context, ex);
            }
        }
        else
        {
            await _next(context);
            return;
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int status;
        var stackTrace = string.Empty;
        string message;

        var exceptionType = exception.GetType();
        if (exceptionType == typeof(ConflictException))
            status = (int)HttpStatusCode.Conflict;
        else if (
            exceptionType == typeof(BadRequestException)
            || exceptionType == typeof(FormatException)
            || exceptionType == typeof(ValidationException)
            || exceptionType == typeof(BadHttpRequestException)
        )
            status = (int)HttpStatusCode.BadRequest;
        else if (exceptionType == typeof(NotFoundException))
            status = (int)HttpStatusCode.NotFound;
        else if (exceptionType == typeof(UnauthorizedException))
            status = (int)HttpStatusCode.Unauthorized;
        else if (exceptionType == typeof(ForbiddenException))
            status = (int)HttpStatusCode.Forbidden;
        else
            status =
                context.Response.StatusCode < 400
                    ? (int)HttpStatusCode.InternalServerError
                    : context.Response.StatusCode;

        message = exception.Message;
        stackTrace = exception.StackTrace;
        var error = exception.GetType().Name;

        var exceptionResult = JsonSerializer.Serialize(
            new ErrorResponse
            {
                Message = message,
                // StackTrace = stackTrace,
                StatusCode = status,
                Error = error,
            }
        );
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        return context.Response.WriteAsync(exceptionResult);
    }

    private static dynamic TransformResponse(HttpContext context, string responseBody)
    {
        dynamic? originalResponse;
        try
        {
            // Parse the original response
            originalResponse = JsonSerializer.Deserialize<dynamic>(responseBody);
        }
        catch
        {
            // Console.WriteLine("Error parsing JSON response");

            // Fallback for non-JSON responses or parsing errors
            originalResponse = responseBody;
        }
        // Create the new response object
        var transformedResponse = new Response
        {
            StatusCode = context.Response.StatusCode,
            Message = "Successful",
        };

        if (originalResponse is null || originalResponse as string == string.Empty)
            return transformedResponse;

        var responseType = originalResponse.GetType();

        // Dynamically create a DataResponse with the appropriate generic type
        var dataResponseType = typeof(DataResponse<>).MakeGenericType(responseType);

        // Create an instance of DataResponse with the correct type
        transformedResponse = Activator.CreateInstance(dataResponseType);

        // Set properties dynamically using reflection
        dataResponseType
            .GetProperty("StatusCode")
            ?.SetValue(transformedResponse, context.Response.StatusCode);
        dataResponseType.GetProperty("Message")?.SetValue(transformedResponse, "Successful");
        dataResponseType.GetProperty("Data")?.SetValue(transformedResponse, originalResponse);

        return transformedResponse;
    }
}
