using DeepgramSharp.Entities;

namespace DeepgramSharp.EventArgs
{
    /// <summary>
    /// Represents the event arguments for when a Deepgram livestream receives a transcript.
    /// </summary>
    public sealed class DeepgramLivestreamTranscriptReceivedEventArgs : DeepgramEventArgs
    {
        /// <summary>
        /// The result received from the Deepgram websocket API.
        /// </summary>
        public required DeepgramLivestreamResult Result { get; init; }
    }
}
