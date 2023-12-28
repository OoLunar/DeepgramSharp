using System.Linq;

namespace DeepgramSharp.Entities
{
    public sealed record DeepgramTranscription
    {
        public required DeepgramMetadata Metadata { get; init; }
        public required DeepgramResult Results { get; init; }

        /// <summary>
        /// The text of the first channel with the highest confidence.
        /// </summary>
        public string? Text => Results.Channels[0].Alternatives.OrderByDescending(x => x.Confidence).FirstOrDefault()?.Transcript;
    }
}
