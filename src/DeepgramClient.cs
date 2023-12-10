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
    public sealed class DeepgramClient
    {
        internal const string BASE_URL = "https://api.deepgram.com";
        internal static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        private static readonly string _defaultUserAgent = $"DeepgramSharp/{typeof(DeepgramClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.1.0"}";

        public Uri BaseUri { get; init; } = new Uri(BASE_URL);
        public DeepgramPreRecordedApi PreRecordedApi { get; init; }
        internal readonly ILogger<DeepgramClient> Logger;
        private readonly AuthenticationHeaderValue _authenticationHeader;

        public DeepgramClient(string apiKey, Uri? baseUri = null, ILogger<DeepgramClient>? logger = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(apiKey, nameof(apiKey));
            _authenticationHeader = new("Token", apiKey);
            PreRecordedApi = new(this, baseUri);
            Logger = logger ?? NullLogger<DeepgramClient>.Instance;
            if (baseUri is not null)
            {
                BaseUri = baseUri;
            }
        }

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
