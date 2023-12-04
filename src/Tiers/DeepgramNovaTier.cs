namespace DeepgramSharp.Tiers
{
    public sealed record DeepgramNovaTier : IDeepgramTier<DeepgramNovaModel>
    {
        public string Tier { get; init; } = "nova";
        public DeepgramNovaModel Model { get; init; } = DeepgramNovaModel.General;
    }

    public enum DeepgramNovaModel
    {
        General,
        Phonecall
    }
}
