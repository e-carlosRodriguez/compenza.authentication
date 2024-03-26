using System.Net;

namespace compenza.authentication.application.Exceptions
{
    public class HttpException: Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public object? Errors { get; set; }

        public HttpException(HttpStatusCode statusCode, string message, object? errors)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}
