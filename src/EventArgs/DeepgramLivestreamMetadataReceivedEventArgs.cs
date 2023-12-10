using DeepgramSharp.Entities;

namespace DeepgramSharp.EventArgs
{
    public sealed class DeepgramLivestreamMetadataReceivedEventArgs : DeepgramEventArgs
    {
        public required DeepgramMetadata Metadata { get; init; }
    }
}
