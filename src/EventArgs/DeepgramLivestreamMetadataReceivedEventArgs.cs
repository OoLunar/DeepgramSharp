using DeepgramSharp.Entities;

namespace DeepgramSharp.EventArgs
{
    /// <summary>
    /// Represents the event arguments for when a Deepgram livestream receives metadata.
    /// </summary>
    public sealed class DeepgramLivestreamMetadataReceivedEventArgs : DeepgramEventArgs
    {
        /// <summary>
        /// The metadata received from the Deepgram websocket API.
        /// </summary>
        public required DeepgramMetadata Metadata { get; init; }
    }
}
