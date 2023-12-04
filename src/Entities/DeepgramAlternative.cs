using System.Collections.Generic;

namespace DeepgramSharp.Entities
{
    public sealed record DeepgramAlternative
    {
        public required string Transcript { get; init; }
        public required double Confidence { get; init; }
        public required IReadOnlyList<DeepgramWord> Words { get; init; }
    }
}
