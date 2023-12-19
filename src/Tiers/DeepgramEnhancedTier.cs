namespace DeepgramSharp.Tiers
{
    /// <summary>
    /// Enhanced model tiers are still some of our most powerful ASR models; they generally have higher accuracy and better word recognition than our base models, and they handle uncommon words significantly better.
    /// </summary>
    public sealed record DeepgramEnhancedTier : IDeepgramTier<DeepgramEnhancedModel>
    {
        /// <inheritdoc cref="IDeepgramTier.Tier"/>
        public string Tier { get; init; } = "enhanced";

        /// <inheritdoc cref="IDeepgramTier.Model"/>
        public DeepgramEnhancedModel Model { get; init; } = DeepgramEnhancedModel.General;
    }

    /// <inheritdoc cref="DeepgramEnhancedTier"/>
    public enum DeepgramEnhancedModel
    {
        /// <summary>
        /// Optimized for everyday audio processing. Likely to be more accurate than any region-specific Base model for the language for which it is enabled. If you aren't sure which model to select, start here.
        /// </summary>
        General,

        /// <summary>
        /// BETA: Optimized for conference room settings, which include multiple speakers with a single microphone.
        /// </summary>
        Meeting,

        /// <summary>
        /// Optimized for low-bandwidth audio phone calls.
        /// </summary>
        Phonecall,

        /// <summary>
        /// BETA: Optimized for multiple speakers with varying audio quality, such as might be found on a typical earnings call. Vocabulary is heavily finance oriented.
        /// </summary>
        Finance
    }
}
