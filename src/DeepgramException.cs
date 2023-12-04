using System;
using System.Net.Http;
using System.Text.Json;

namespace DeepgramSharp
{
    public sealed class DeepgramException : Exception
    {
        public HttpResponseMessage? Response { get; init; }
        public string? ErrorCode { get; init; }
        public string? ErrorMessage { get; init; }
        public Guid RequestId { get; init; }
        public override string Message => $"Http Error {(int)(Response?.StatusCode ?? 0)}, {ErrorCode}: {ErrorMessage}";

        internal DeepgramException(JsonDocument jsonDocument, HttpResponseMessage? response = null)
        {
            ArgumentNullException.ThrowIfNull(jsonDocument, nameof(jsonDocument));

            Response = response;
            if (jsonDocument.RootElement.TryGetProperty("err_code", out JsonElement errorCodeElement))
            {
                ErrorCode = errorCodeElement.GetString();
            }

            if (jsonDocument.RootElement.TryGetProperty("err_msg", out JsonElement errorMessageElement))
            {
                ErrorMessage = errorMessageElement.GetString();
            }

            if (jsonDocument.RootElement.TryGetProperty("request_id", out JsonElement requestIdElement))
            {
                RequestId = requestIdElement.GetGuid();
            }
        }
    }
}
