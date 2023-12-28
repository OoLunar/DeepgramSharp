using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeepgramSharp
{
    /// <summary>
    /// Represents the Deepgram API.
    /// </summary>
    public sealed class DeepgramClient
    {
        internal static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        private static readonly string _defaultUserAgent = $"DeepgramSharp/{typeof(DeepgramClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.1.0"}";

        /// <summary>
        /// The base URI to use for requests.
        /// </summary>
        public Uri BaseUri { get; init; }

        /// <summary>
        /// The <see cref="DeepgramPreRecordedApi"/> to used for transcribing prerecorded audio.
        /// </summary>
        public DeepgramPreRecordedApi PreRecordedApi { get; init; }
        internal readonly ILogger<DeepgramClient> Logger;
        private readonly AuthenticationHeaderValue _authenticationHeader;

        /// <summary>
        /// Creates a new <see cref="DeepgramClient"/> and connects to the Deepgram prerecorded API.
        /// </summary>
        /// <param name="apiKey">The API key to use for requests.</param>
        /// <param name="baseUri">The URI to use for requests. Overwrites the default Uri, <see cref="DeepgramRoutes.PrerecordedUri"/>.</param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> to use for logging.</param>
        public DeepgramClient(string apiKey, ILogger<DeepgramClient>? logger = null, Uri? baseUri = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(apiKey, nameof(apiKey));
            _authenticationHeader = new("Token", apiKey);
            PreRecordedApi = new(this, baseUri);
            BaseUri = baseUri ?? DeepgramRoutes.PrerecordedUri;
            Logger = logger ?? NullLogger<DeepgramClient>.Instance;
        }

        /// <summary>
        /// Creates a new <see cref="DeepgramLivestreamApi"/> and connects to the Deepgram livestream API.
        /// </summary>
        /// <param name="options">The options to use for the livestream.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for the request.</param>
        /// <returns>A <see cref="DeepgramLivestreamApi"/> connected to the Deepgram livestream API.</returns>
        public async ValueTask<DeepgramLivestreamApi> CreateLivestreamAsync(DeepgramLivestreamOptionCollection? options = null, CancellationToken cancellationToken = default)
        {
            DeepgramLivestreamApi api = new(this, BaseUri);
            await api.ConnectAsync(options, cancellationToken);
            return api;
        }

        internal void SetDefaultHeaders(HttpRequestMessage request)
        {
            request.Headers.UserAgent.ParseAdd(_defaultUserAgent);
            request.Headers.Authorization = _authenticationHeader;
        }
    }
}
