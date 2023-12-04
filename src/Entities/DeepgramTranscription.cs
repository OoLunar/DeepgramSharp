namespace DeepgramSharp.Entities
{
    public sealed record DeepgramTranscription
    {
        public required DeepgramMetadata Metadata { get; init; }
        public required DeepgramResult Results { get; init; }
    }
}
