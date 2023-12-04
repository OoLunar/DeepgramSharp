namespace DeepgramSharp.Entities
{
    public sealed record DeepgramWord
    {
        public required string Word { get; init; }
        public required double Confidence { get; init; }
        public required double Start { get; init; }
        public required double End { get; init; }
    }
}
