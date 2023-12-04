using System;

namespace DeepgramSharp.Entities
{
    public sealed record DeepgramUrlPayload
    {
        public required Uri Url { get; init; }
    }
}
