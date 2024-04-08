using System.Net;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace compenza.authentication.application.Exceptions
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
         
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Capturar el body de la solicitud entrante
                var requestBodyStream = new MemoryStream();
                var originalRequestBody = context.Request.Body;

                await context.Request.Body.CopyToAsync(requestBodyStream);
                requestBodyStream.Seek(0, SeekOrigin.Begin);
                var requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();

                Log.Information($"Body: {requestBodyText}");

                requestBodyStream.Seek(0, SeekOrigin.Begin);
                context.Request.Body = requestBodyStream;

                await _next(context);          
            }
            catch (Exception exception)
            {
                Log.Error("Exception error: {@exception}", exception);
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var errorResponse = new ErrorResponse();

            if (exception is HttpException httpException)
            {
                errorResponse.StatusCode = httpException.StatusCode;
                errorResponse.Message = httpException.Message;
                errorResponse.Errors = httpException.Errors;
            }
            else
            {
                errorResponse.StatusCode = HttpStatusCode.InternalServerError;
                errorResponse.Message = exception.Message;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)errorResponse.StatusCode;

            //eSoftware.Utilerias.ControlErrores.LogError(context.Response.StatusCode.ToString(), exception, errorResponse.Message);
            await context.Response.WriteAsync(errorResponse.ToJsonString());
        }
    }
}
