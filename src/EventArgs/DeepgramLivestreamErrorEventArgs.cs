using DeepgramSharp.Exceptions;

namespace DeepgramSharp.EventArgs
{
    /// <summary>
    /// Represents the event arguments for when a Deepgram livestream encounters an error.
    /// </summary>
    public sealed class DeepgramLivestreamErrorEventArgs : DeepgramEventArgs
    {
        /// <summary>
        /// The exception thrown by the Deepgram websocket API.
        /// </summary>
        public required DeepgramWebsocketException Exception { get; init; }
    }
}
