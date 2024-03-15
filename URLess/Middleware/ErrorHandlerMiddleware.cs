using Newtonsoft.Json;
using System.Net;

namespace URLess.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentNullException badRequest)
            {
                await ExceptionHandler(badRequest, context, HttpStatusCode.BadRequest);
            }
            catch (UnauthorizedAccessException unauthorized)
            {
                await ExceptionHandler(unauthorized, context, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                await ExceptionHandler(ex, context, HttpStatusCode.InternalServerError);
            }
        }

        private async Task ExceptionHandler(Exception exception, HttpContext context, HttpStatusCode statusCode)
        {
            _logger.LogError(exception, "error during executing {context}", context.Request.Path.Value);

            var response = context.Response;
            response.ContentType = "application/json";

            string message = exception.Message;
            response.StatusCode = (int)statusCode;

            var result = JsonConvert.SerializeObject(message);

            await response.WriteAsync(result);
        }
    }
}
