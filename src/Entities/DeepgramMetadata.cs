using System;
using System.Collections.Generic;

namespace DeepgramSharp.Entities
{
    public sealed record DeepgramMetadata
    {
        public required Guid RequestId { get; init; }
        public required string Sha256 { get; init; }
        public required DateTimeOffset Created { get; init; }
        public required double Duration { get; init; }
        public required int Channels { get; init; }
        public IReadOnlyList<string> Models { get; init; } = new List<string>();
        public IReadOnlyDictionary<Guid, DeepgramModelInfo> ModelInfo { get; init; } = new Dictionary<Guid, DeepgramModelInfo>();
    }
}
