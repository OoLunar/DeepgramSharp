using System.Diagnostics;
using System.Text.Json;

namespace DeepgramSharp.Exceptions
{
    public sealed class DeepgramWebsocketException : DeepgramException
    {
        public DeepgramWebsocketException(JsonDocument jsonDocument) : base(jsonDocument) => Debugger.Break();
    }
}
