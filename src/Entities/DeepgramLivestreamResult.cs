using System.Collections.Generic;

namespace DeepgramSharp.Entities
{
    public sealed record DeepgramLivestreamResult
    {
        public required IReadOnlyList<int> ChannelIndex { get; init; }
        public required double Duration { get; init; }
        public required double Start { get; init; }
        public required bool IsFinal { get; init; }
        public required bool SpeechFinal { get; init; }
        public required DeepgramChannel Channel { get; init; }
    }
}
