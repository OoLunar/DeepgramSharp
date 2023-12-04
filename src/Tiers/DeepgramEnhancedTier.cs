namespace DeepgramSharp.Tiers
{
    public sealed record DeepgramEnhancedTier : IDeepgramTier<DeepgramEnhancedModel>
    {
        public string Tier { get; init; } = "enhanced";
        public DeepgramEnhancedModel Model { get; init; } = DeepgramEnhancedModel.General;
    }

    public enum DeepgramEnhancedModel
    {
        General,
        Meeting,
        Phonecall,
        Finance
    }
}
