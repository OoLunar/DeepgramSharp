using System;

namespace DeepgramSharp
{
    /// <summary>
    /// A class containing the routes for the Deepgram API.
    /// </summary>
    public static class DeepgramRoutes
    {
        /// <summary>
        /// The base URL for the Deepgram API.
        /// </summary>
        public const string BASE_URL = "https://api.deepgram.com";

        /// <summary>
        /// The latest version of the Deepgram API.
        /// </summary>
        public const string LATEST_VERSION = "/v1";

        /// <summary>
        /// The path to the prerecorded audio endpoint.
        /// </summary>
        public const string LISTEN = "/listen";

        /// <inheritdoc cref="BASE_URL"/>
        public static readonly Uri BaseUri = new(BASE_URL + LATEST_VERSION);

        /// <summary>
        /// The URI for the prerecorded audio endpoint.
        /// </summary>
        public static readonly Uri PrerecordedUri = new(BASE_URL + LATEST_VERSION + LISTEN);

        /// <summary>
        /// The URI for the livestream audio endpoint.
        /// </summary>
        public static readonly Uri LivestreamUri = new UriBuilder(PrerecordedUri)
        {
            Scheme = "wss"
        }.Uri;
    }
}
