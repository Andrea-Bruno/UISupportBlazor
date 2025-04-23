using Microsoft.AspNetCore.Http;

/// <summary>
/// Middleware to handle HTTP POST requests to the "/api" endpoint.
/// It processes the request body, executes a command using the provided API command set,
/// and returns a JSON response.
/// </summary>
public class ApiMiddleware
{
    // Delegate to invoke the next middleware in the pipeline.
    private readonly RequestDelegate _next;

    // The type containing the set of API commands to execute.
    private Type ApiCommandSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="apiCommandSet">The type containing the API commands to execute.</param>
    public ApiMiddleware(RequestDelegate next, Type apiCommandSet)
    {
        ApiCommandSet = apiCommandSet;
        _next = next;
    }

    /// <summary>
    /// Middleware logic to handle incoming HTTP requests.
    /// If the request is a POST to "/api", it processes the request body,
    /// executes the corresponding API command, and writes the response.
    /// Otherwise, it passes the request to the next middleware in the pipeline.
    /// </summary>
    /// <param name="context">The HTTP context of the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the request is a POST to the "/api" endpoint.
        if (context.Request.Path == "/api" && context.Request.Method == HttpMethods.Post)
        {
            // Read the request body as a string.
            using var reader = new StreamReader(context.Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            // Set the response content type to JSON.
            context.Response.ContentType = "application/json";

            // Execute the API command and set the response status code and body.
            context.Response.StatusCode = (int)UISupportGeneric.API.Post(ApiCommandSet, requestBody, out string response);
            await context.Response.WriteAsync(response);
            return; // End the middleware pipeline for this request.
        }

        // If the request does not match, pass it to the next middleware.
        await _next(context);
    }
}
