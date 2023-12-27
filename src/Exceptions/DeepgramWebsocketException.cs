using System.Text.Json;

namespace DeepgramSharp.Exceptions
{
    /// <summary>
    /// Represents an exception thrown by the Deepgram websocket API.
    /// </summary>
    public sealed class DeepgramWebsocketException : DeepgramException
    {
        /// <summary>
        /// Creates a new <see cref="DeepgramWebsocketException"/>.
        /// </summary>
        /// <param name="jsonDocument">The <see cref="JsonDocument"/> containing the error.</param>
        /// <returns>A new <see cref="DeepgramWebsocketException"/>.</returns>
        internal DeepgramWebsocketException(JsonDocument jsonDocument) : base(jsonDocument) { }
    }
}
