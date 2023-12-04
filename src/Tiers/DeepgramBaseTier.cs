namespace DeepgramSharp.Tiers
{
    public sealed record DeepgramBaseTier : IDeepgramTier<DeepgramBaseModel>
    {
        public string Tier { get; init; } = "base";
        public DeepgramBaseModel Model { get; init; } = DeepgramBaseModel.General;
    }

    public enum DeepgramBaseModel
    {
        General,
        Meeting,
        Phonecall,
        Voicemail,
        Finance,
        ConversationalAI,
        Video
    }
}
