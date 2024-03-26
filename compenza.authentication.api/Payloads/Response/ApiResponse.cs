using System.Net;
using System.Text.Json.Serialization;

namespace compenza.authentication.api.Payloads.Response
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("StatusCode")]
        public HttpStatusCode StatusCode { get; set; }

        [JsonPropertyName("Message")]
        public string Message { get; set; }

        [JsonPropertyName("Data")]
        public T Data { get; set; }

        public ApiResponse(T data)
        {
            this.Data = data;
        }
    }
}
