using DeepgramSharp.Exceptions;

namespace DeepgramSharp.EventArgs
{
    public sealed class DeepgramLivestreamErrorEventArgs : DeepgramEventArgs
    {
        public required DeepgramWebsocketException Error { get; init; }
    }
}
