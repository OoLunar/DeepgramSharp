using System;

namespace DeepgramSharp
{
    public static class DeepgramRoutes
    {
        public const string BASE_URL = "https://api.deepgram.com";
        public const string LATEST_VERSION = "/v1";
        public const string LISTEN = "/listen";

        public static readonly Uri BaseUri = new(BASE_URL + LATEST_VERSION);
        public static readonly Uri PrerecordedUri = new(BASE_URL + LATEST_VERSION + LISTEN);
        public static readonly Uri LivestreamUri = new UriBuilder(PrerecordedUri)
        {
            Scheme = "wss"
        }.Uri;
    }
}
