using DeepgramSharp.Entities;

namespace DeepgramSharp.EventArgs
{
    public sealed class DeepgramLivestreamTranscriptReceivedEventArgs : DeepgramEventArgs
    {
        public required DeepgramLivestreamResult Result { get; init; }
    }
}
