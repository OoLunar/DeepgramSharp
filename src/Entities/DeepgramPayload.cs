namespace DeepgramSharp.Entities
{
    internal sealed record DeepgramWebsocketPayload
    {
        public required string Type { get; init; }
    }

    /// <summary>
    /// Informs the server of an update in the connection state.
    /// </summary>
    public enum DeepgramPayloadType
    {
        /// <summary>
        /// Keep the connection alive for another 12 seconds.
        /// </summary>
        KeepAlive,

        /// <summary>
        /// The connection is closing.
        /// </summary>
        CloseStream
    }
}
