namespace DeepgramSharp.Tiers
{
    /// <summary>
    /// Nova-2 expands on <see cref="DeepgramNovaTier"/> advancements with speech-specific optimizations to the underlying Transformer architecture, advanced data curation techniques, and a multi-stage training methodology. These changes yield reduced word error rate (WER) and enhancements to entity recognition (i.e. proper nouns, alphanumerics, etc.), punctuation, and capitalization.
    /// </summary>
    public sealed record DeepgramNova2Tier : IDeepgramTier<DeepgramNova2Model>
    {
        /// <inheritdoc cref="IDeepgramTier.Tier"/>
        public string Tier { get; init; } = "nova-2";

        /// <inheritdoc cref="IDeepgramTier.Model"/>
        public DeepgramNova2Model Model { get; init; } = DeepgramNova2Model.General;
    }

    /// <inheritdoc cref="DeepgramNova2Tier"/>
    public enum DeepgramNova2Model
    {
        /// <summary>
        /// Optimized for everyday audio processing.
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
        Video,

        /// <summary>
        /// Optimized for audio with medical oriented vocabulary.
        /// </summary>
        Medical,

        /// <summary>
        /// Optimized for audio sources from drivethrus.
        /// </summary>
        DriveThru,

        /// <summary>
        /// Optimized for audio with automative oriented vocabulary.
        /// </summary>
        Automotive
    }
}
