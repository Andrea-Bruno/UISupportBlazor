using Microsoft.AspNetCore.Http;


namespace UISupportBlazor
{
    /// <summary>
    /// Middleware to handle HTTP POST requests to a configurable endpoint.
    /// It processes the request body, executes a command using the provided API command set,
    /// and returns a JSON response.
    /// </summary>
    public class ApiMiddleware
    {
        // Delegate to invoke the next middleware in the pipeline.
        private readonly RequestDelegate _next;

        // The type containing the set of API commands to execute.
        private Type _apiCommandSet;

        // The API endpoint path.
        private readonly string _apiPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="apiCommandSet">The type containing the API commands to execute.</param>
        /// <param name="apiPath">The path where the middleware should intercept POST requests.</param>
        public ApiMiddleware(RequestDelegate next, Type apiCommandSet, string? apiPath = "/api")
        {
            if (apiPath?.StartsWith("/") != true)
                apiPath = "/" + apiPath; // Ensure the path starts with a "/".
            _apiCommandSet = apiCommandSet;
            _next = next;
            _apiPath = apiPath ?? "/api"; // Ensure default path if none is provided.
        }

        /// <summary>
        /// Middleware logic to handle incoming HTTP requests.
        /// If the request is a POST to the configured API endpoint, it processes the request body,
        /// executes the corresponding API command, and writes the response.
        /// Otherwise, it passes the request to the next middleware in the pipeline.
        /// </summary>
        /// <param name="context">The HTTP context of the current request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request is a POST to the configured endpoint.
            if (context.Request.Path.StartsWithSegments(_apiPath))
            {
                if (context.Request.Method == HttpMethods.Post)
                {
                    var segments = context.Request.Path.Value.Split('/');
                    var methodName = segments.Length > 2 ? segments[2] : null;

                    // Read the request body as a string.
                    using var reader = new StreamReader(context.Request.Body);
                    var requestBody = await reader.ReadToEndAsync();

                    // Set the response content type to JSON.
                    context.Response.ContentType = "application/json";

                    // Execute the API command and set the response status code and body.
                    context.Response.StatusCode = (int)UISupportGeneric.API.Post(_apiCommandSet, methodName, requestBody, out string response);
                    await context.Response.WriteAsync(response);
                    return; // End the middleware pipeline for this request.
                }
                else if (context.Request.Method == HttpMethods.Get)
                {
                    // Set the response content type text/plain for c# code.
                    context.Response.ContentType = "text/plain";

                    context.Response.StatusCode = (int)UISupportGeneric.API.Get(_apiCommandSet, out string response);
                    await context.Response.WriteAsync(response);
                    return; // End the middleware pipeline for this request.
                }
            }
            try
            {
                // If the request does not match, pass it to the next middleware.
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync($"{{\"error\": \"Bad Request: {ex.Message}\"}}");
            }
        }
    }
}