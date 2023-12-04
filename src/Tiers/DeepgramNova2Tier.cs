namespace DeepgramSharp.Tiers
{
    public sealed record DeepgramNova2Tier : IDeepgramTier<DeepgramNova2Model>
    {
        public string Tier { get; init; } = "nova-2";
        public DeepgramNova2Model Model { get; init; } = DeepgramNova2Model.General;
    }

    public enum DeepgramNova2Model
    {
        General,
        Meeting,
        Phonecall,
        Voicemail,
        Finance,
        ConversationalAI,
        Video,
        Medical,
        DriveThru,
        Automotive
    }
}
