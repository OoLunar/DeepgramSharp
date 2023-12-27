namespace DeepgramSharp.Entities
{
    /// <summary>
    /// The type of response from a Deepgram livestream connection.
    /// </summary>
    public enum DeepgramLivestreamResponseType
    {
        /// <summary>
        /// The connection has been closed.
        /// </summary>
        Closed = 0,

        /// <summary>
        /// Metadata about the connection.
        /// </summary>
        Metadata = 1,

        /// <summary>
        /// A transcript of the audio sent to the websocket.
        /// </summary>
        Transcript = 2,

        /// <summary>
        /// An error occurred during the livestream connection.
        /// </summary>
        Error = 3
    }
}
