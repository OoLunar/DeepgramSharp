using System;
using System.Net.Http;
using System.Text.Json;

namespace DeepgramSharp.Exceptions
{
    /// <summary>
    /// Represents an exception thrown by the Deepgram API.
    /// </summary>
    public class DeepgramException : Exception
    {
        /// <summary>
        /// The error code provided by the Deepgram API.
        /// </summary>
        public string? ErrorCode { get; init; }

        /// <summary>
        /// The error message provided by the Deepgram API.
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// The request ID provided by the Deepgram API.
        /// </summary>
        public Guid RequestId { get; init; }

        /// <summary>
        /// The <see cref="HttpResponseMessage"/> that caused the exception.
        /// </summary>
        public HttpResponseMessage? Response { get; init; }

        /// <inheritdoc/>
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
