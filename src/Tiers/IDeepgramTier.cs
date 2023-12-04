using System;

namespace DeepgramSharp.Tiers
{
    public interface IDeepgramTier
    {
        public string Name => $"{Tier}-{Model.ToString().ToLowerInvariant()}";
        public string Tier { get; }
        public Enum Model { get; }
    }

    public interface IDeepgramTier<T> : IDeepgramTier where T : struct, Enum
    {
        public new T Model { get; }
        Enum IDeepgramTier.Model => Model;
    }
}
