using System.Net;
using System.Runtime.Serialization;

namespace compenza.authentication.domain.Configure
{
    [Serializable]
    internal class HttpException : Exception
    {
        private HttpStatusCode locked;
        private string v;
        private object errorDetails;

        public HttpException()
        {
        }

        public HttpException(string? message) : base(message)
        {
        }

        public HttpException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public HttpException(HttpStatusCode locked, string v, object errorDetails)
        {
            this.locked = locked;
            this.v = v;
            this.errorDetails = errorDetails;
        }

        protected HttpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}