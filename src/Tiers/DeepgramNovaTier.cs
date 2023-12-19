namespace DeepgramSharp.Tiers
{
    /// <summary>
    /// Nova is the predecessor to <see cref="DeepgramNova2Tier"/>. Training on this model spans over 100 domains and 47 billion tokens, making it the deepest-trained automatic speech recognition (ASR) model to date. Nova doesn't just excel in one specific domain — it is ideal for a wide array of voice applications that require high accuracy in diverse contexts.
    /// </summary>
    /// <value></value>
    public sealed record DeepgramNovaTier : IDeepgramTier<DeepgramNovaModel>
    {
        /// <inheritdoc cref="IDeepgramTier.Tier"/>
        public string Tier { get; init; } = "nova";

        /// <inheritdoc cref="IDeepgramTier.Model"/>
        public DeepgramNovaModel Model { get; init; } = DeepgramNovaModel.General;
    }

    /// <inheritdoc cref="DeepgramNovaTier"/>
    public enum DeepgramNovaModel
    {
        /// <summary>
        /// Optimized for everyday audio processing. Likely to be more accurate than any region-specific Base model for the language for which it is enabled. If you aren't sure which model to select, start here.
        /// </summary>
        General,

        /// <summary>
        /// Optimized for low-bandwidth audio phone calls.
        /// </summary>
        Phonecall
    }
}
