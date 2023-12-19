using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DeepgramSharp.Entities;
using DeepgramSharp.Exceptions;

namespace DeepgramSharp
{
    /// <summary>
    /// Represents the Deepgram prerecorded API.
    /// </summary>
    /// <param name="client">The <see cref="DeepgramClient"/> to use for requests.</param>
    /// <param name="baseUri">The URI to use for requests. Overwrites the default Uri, <see cref="DeepgramRoutes.PrerecordedUri"/>.</param>
    public sealed class DeepgramPreRecordedApi(DeepgramClient client, Uri? baseUri = null)
    {
        private static readonly HttpClient HttpClient = new();

        /// <summary>
        /// The URI to use for requests.
        /// </summary>
        public Uri BaseUri { get; init; } = baseUri ?? DeepgramRoutes.PrerecordedUri;

        /// <summary>
        /// The <see cref="DeepgramClient"/> to use for requests.
        /// </summary>
        public DeepgramClient Client { get; init; } = client ?? throw new ArgumentNullException(nameof(client));

        /// <summary>
        /// Transcribes the audio from the given stream.
        /// </summary>
        /// <param name="audioStream">The stream containing the audio to transcribe.</param>
        /// <param name="options">A collection of options to use for the request.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for the request.</param>
        /// <returns>A <see cref="DeepgramTranscription"/> containing the transcription.</returns>
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

        /// <inheritdoc cref="TranscribeAsync(Stream, DeepgramPrerecordedApiOptionCollection?, CancellationToken)"/>
        /// <param name="url">The URL that Deepgram should use to download the audio.</param>
        /// <param name="options">A collection of options to use for the request.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for the request.</param>
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
