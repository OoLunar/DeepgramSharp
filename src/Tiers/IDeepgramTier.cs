using System;

namespace DeepgramSharp.Tiers
{
    /// <summary>
    /// Represents a Deepgram tier.
    /// </summary>
    public interface IDeepgramTier
    {
        /// <summary>
        /// The fullname of the Deepgram tier (e.g. "nova2-general").
        /// </summary>
        public string FullName => $"{Tier}-{Model.ToString().ToLowerInvariant()}";

        /// <summary>
        /// The tier of the Deepgram model (e.g. "nova2").
        /// </summary>
        public string Tier { get; }

        /// <summary>
        /// The model of the Deepgram tier (e.g. "general").
        /// </summary>
        public Enum Model { get; }
    }

    /// <summary>
    /// A strongly-typed Deepgram tier.
    /// </summary>
    /// <typeparam name="T">The type of the Deepgram model.</typeparam>
    public interface IDeepgramTier<T> : IDeepgramTier where T : struct, Enum
    {
        /// <inheritdoc cref="IDeepgramTier.Model"/>
        public new T Model { get; }
        Enum IDeepgramTier.Model => Model;
    }
}
