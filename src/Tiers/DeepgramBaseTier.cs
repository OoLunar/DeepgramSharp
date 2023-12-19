namespace DeepgramSharp.Tiers
{
    /// <summary>
    /// Base model tiers are built on our signature end-to-end deep learning speech model architecture. They offer a solid combination of accuracy and cost effectiveness in some cases.
    /// </summary>
    public sealed record DeepgramBaseTier : IDeepgramTier<DeepgramBaseModel>
    {
        /// <inheritdoc cref="IDeepgramTier.Tier"/>
        public string Tier { get; init; } = "base";

        /// <inheritdoc cref="IDeepgramTier.Model"/>
        public DeepgramBaseModel Model { get; init; } = DeepgramBaseModel.General;
    }

    /// <inheritdoc cref="DeepgramBaseTier"/>
    public enum DeepgramBaseModel
    {
        /// <summary>
        /// (Default) Optimized for everyday audio processing.
        /// </summary>
        General,

        /// <summary>
        /// Optimized for conference room settings, which include multiple speakers with a single microphone.
        /// </summary>
        Meeting,

        /// <summary>
        /// Optimized for low-bandwidth audio phone calls.
        /// </summary>
        Phonecall,

        /// <summary>
        /// Optimized for low-bandwidth audio clips with a single speaker. Derived from the phonecall model.
        /// </summary>
        Voicemail,

        /// <summary>
        /// Optimized for multiple speakers with varying audio quality, such as might be found on a typical earnings call. Vocabulary is heavily finance oriented.
        /// </summary>
        Finance,

        /// <summary>
        /// Optimized for use cases in which a human is talking to an automated bot, such as IVR, a voice assistant, or an automated kiosk.
        /// </summary>
        ConversationalAI,

        /// <summary>
        /// Optimized for audio sourced from videos.
        /// </summary>
        Video
    }
}
