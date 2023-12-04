using System.Collections.Generic;

namespace DeepgramSharp.Entities
{
    public sealed record DeepgramChannel
    {
        public required IReadOnlyList<DeepgramAlternative> Alternatives { get; init; }
    }
}
