namespace DeepgramSharp.Entities
{
    public sealed record DeepgramModelInfo
    {
        public required string Name { get; init; }
        public required string Version { get; init; }
        public required string Arch { get; init; }
    }
}
