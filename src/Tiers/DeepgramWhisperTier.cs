namespace DeepgramSharp.Tiers
{
    public sealed record DeepgramWhisperTier : IDeepgramTier<DeepgramWhisperModel>
    {
        public string Tier => $"whisper-{Model.ToString().ToLowerInvariant()}";
        public DeepgramWhisperModel Model { get; init; } = DeepgramWhisperModel.Medium;
    }

    public enum DeepgramWhisperModel
    {
        Tiny,
        Base,
        Small,
        Medium,
        Large
    }
}
