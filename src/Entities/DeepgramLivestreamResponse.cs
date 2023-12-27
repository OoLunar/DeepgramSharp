using System;
using DeepgramSharp.Exceptions;

namespace DeepgramSharp.Entities
{
    /// <summary>
    /// The response from a Deepgram livestream request
    /// </summary>
    public sealed record DeepgramLivestreamResponse
    {
        /// <summary>
        /// The type of response
        /// </summary>
        public DeepgramLivestreamResponseType Type { get; init; }

        /// <summary>
        /// The metadata associated with the websocket connection.
        /// </summary>
        /// <remarks>
        /// Not null if <see cref="Type"/> is <see cref="DeepgramLivestreamResponseType.Metadata"/>
        /// </remarks>
        public DeepgramMetadata? Metadata { get; init; }

        /// <summary>
        /// The transcript of the audio sent to the websocket.
        /// </summary>
        /// <remarks>
        /// Not null if <see cref="Type"/> is <see cref="DeepgramLivestreamResponseType.Transcript"/>
        /// </remarks>
        public DeepgramLivestreamResult? Transcript { get; init; }

        /// <summary>
        /// An error that occurred during the livestream connection.
        /// </summary>
        /// <remarks>
        /// Not null if <see cref="Type"/> is <see cref="DeepgramLivestreamResponseType.Error"/>
        /// If the error came from Deepgram, this will be a <see cref="DeepgramException"/>
        /// </remarks>
        public Exception? Error { get; init; }
    }
}
