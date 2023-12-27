using System;

namespace DeepgramSharp.Entities
{
    public sealed record DeepgramLivestreamResponse
    {
        public DeepgramLivestreamResponseType Type { get; init; }
        public DeepgramMetadata? Metadata { get; init; }
        public DeepgramLivestreamResult? Transcript { get; init; }
        public Exception? Error { get; init; }
    }
}
