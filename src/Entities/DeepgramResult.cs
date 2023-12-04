using System.Collections.Generic;

namespace DeepgramSharp.Entities
{
    public sealed record DeepgramResult
    {
        public required IReadOnlyList<DeepgramChannel> Channels { get; init; }
    }
}
