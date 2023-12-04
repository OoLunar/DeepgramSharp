using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DeepgramSharp.Entities;

namespace DeepgramSharp
{
    public sealed class DeepgramPreRecordedApi
    {
        private static readonly HttpClient HttpClient = new();

        public Uri BaseUri { get; init; } = new(DeepgramClient.BASE_URL + "/v1/listen");
        public DeepgramClient Client { get; init; }

        public DeepgramPreRecordedApi(DeepgramClient client, Uri? baseUri = null)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            if (baseUri is not null)
            {
                BaseUri = baseUri;
            }
        }

        public ValueTask<DeepgramTranscription?> TranscribeAsync(Stream audioStream, DeepgramPrerecordedApiOptionCollection? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(audioStream);
            HttpRequestMessage request = new(HttpMethod.Post, new UriBuilder(BaseUri) { Query = options?.ToQueryString() ?? "" }.Uri)
            {
                Method = HttpMethod.Post,
                Content = new StreamContent(audioStream)
            };

            return TranscribeAsync(request, cancellationToken);
        }

        public ValueTask<DeepgramTranscription?> TranscribeAsync(Uri url, DeepgramPrerecordedApiOptionCollection? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(url);
            HttpRequestMessage request = new(HttpMethod.Post, new UriBuilder(BaseUri) { Query = options?.ToQueryString() ?? "" }.Uri)
            {
                Method = HttpMethod.Post,
                Content = JsonContent.Create(new DeepgramUrlPayload { Url = url }, MediaTypeHeaderValue.Parse("application/json"), DeepgramClient.DefaultJsonSerializerOptions)
            };

            return TranscribeAsync(request, cancellationToken);
        }

        private async ValueTask<DeepgramTranscription?> TranscribeAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            Client.SetDefaultHeaders(request);
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<DeepgramTranscription>(DeepgramClient.DefaultJsonSerializerOptions, cancellationToken)
                : throw new DeepgramException(await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), default, cancellationToken), response);
        }
    }
}
