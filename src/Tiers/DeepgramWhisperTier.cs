namespace DeepgramSharp.Tiers
{
    /// <summary>
    /// Deepgram's Whisper Cloud is a fully managed API that gives you access to Deepgram's version of OpenAI’s Whisper model. Read our guide Deepgram Whisper Cloud for a deeper dive into this offering: <see href="https://developers.deepgram.com/docs/deepgram-whisper-cloud" />
    /// </summary>
    /// <remarks>
    /// Whisper models are less scalable than all other Deepgram models due to their inherent model architecture. All non-Whisper models will return results faster and scale to higher load.
    /// </remarks>
    public sealed record DeepgramWhisperTier : IDeepgramTier<DeepgramWhisperModel>
    {
        /// <inheritdoc cref="IDeepgramTier.Tier"/>
        public string Tier => $"whisper-{Model.ToString().ToLowerInvariant()}";

        /// <inheritdoc cref="IDeepgramTier.Model"/>
        public DeepgramWhisperModel Model { get; init; } = DeepgramWhisperModel.Medium;
    }

    /// <inheritdoc cref="DeepgramWhisperTier"/>
    public enum DeepgramWhisperModel
    {
        /// <summary>
        /// Contains 39 million parameters. The smallest model available.
        /// </summary>
        Tiny,

        /// <summary>
        /// Contains 74 million parameters.
        /// </summary>
        Base,

        /// <summary>
        /// Contains 244 million parameters.
        /// </summary>
        Small,

        /// <summary>
        /// Contains 769 million parameters. The default model if you don't specify a size.
        /// </summary>
        Medium,

        /// <summary>
        /// Contains 1550 million parameters. The largest model available. Defaults to OpenAI’s Whisper large-v2.
        /// </summary>
        Large
    }
}
