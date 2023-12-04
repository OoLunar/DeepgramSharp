using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DeepgramSharp.Entities;

namespace DeepgramSharp
{
    public sealed class DeepgramLivestreamApi : IDisposable
    {
        private class AuthenticatedMessageInvoker : HttpMessageInvoker
        {
            public DeepgramClient Client { get; init; }

            public AuthenticatedMessageInvoker(DeepgramClient client) : base(new HttpClientHandler())
            {
                ArgumentNullException.ThrowIfNull(client, nameof(client));
                Client = client;
            }

            public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Client.SetDefaultHeaders(request);
                return base.SendAsync(request, cancellationToken);
            }
        }

        private const string BASE_ROUTE = "/v1/listen";
        private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(6);
        private static readonly byte[] KeepAliveMessage = JsonSerializer.SerializeToUtf8Bytes(new DeepgramWebsocketPayload()
        {
            Type = DeepgramPayloadType.KeepAlive.ToString()
        }, DeepgramClient.DefaultJsonSerializerOptions);

        private static readonly byte[] CloseMessage = JsonSerializer.SerializeToUtf8Bytes(new DeepgramWebsocketPayload()
        {
            Type = DeepgramPayloadType.CloseStream.ToString()
        }, DeepgramClient.DefaultJsonSerializerOptions);

        public DeepgramClient Client { get; init; }
        public Uri BaseUri { get; init; } = new(DeepgramClient.BASE_URL + BASE_ROUTE);
        public ClientWebSocket WebSocket { get; init; }
        public WebSocketState State => WebSocket.State;

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private byte[] _buffer = new byte[512];
        private DateTimeOffset _lastKeepAlive = DateTimeOffset.Now;
        private bool _isDisposed;


        public DeepgramLivestreamApi(DeepgramClient client, Uri? baseUri = null)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            if (baseUri is not null)
            {
                BaseUri = new(baseUri, BASE_ROUTE);
            }

            WebSocket = new();
        }

        public async ValueTask ConnectAsync(DeepgramLivestreamOptionCollection? options = null, CancellationToken cancellationToken = default)
        {
            if (WebSocket.State == WebSocketState.Open)
            {
                throw new InvalidOperationException("WebSocket is already open.");
            }

            await WebSocket.ConnectAsync(new UriBuilder(BaseUri)
            {
                Scheme = "wss",
                Query = options?.ToQueryString() ?? ""
            }.Uri, new AuthenticatedMessageInvoker(Client), cancellationToken);
            _ = KeepAliveLoopAsync();
        }

        public async ValueTask SendAudioAsync(ReadOnlyMemory<byte> audioFrames, CancellationToken cancellationToken = default)
        {
            if (WebSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("WebSocket is not open.");
            }

            await WebSocket.SendAsync(audioFrames, WebSocketMessageType.Binary, true, cancellationToken);
            _lastKeepAlive = DateTimeOffset.UtcNow;
        }

        public async ValueTask<DeepgramLivestreamResult?> ReceiveTranscriptionAsync(CancellationToken cancellationToken = default)
        {
            if (WebSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("WebSocket is not open.");
            }

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                // Clear the buffer
                Array.Clear(_buffer, 0, _buffer.Length);
                ValueWebSocketReceiveResult result = await WebSocket.ReceiveAsync(_buffer.AsMemory(), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return null;
                }
                else if (result.MessageType != WebSocketMessageType.Text)
                {
                    throw new InvalidOperationException("WebSocket received unexpected message type.");
                }

                int bytesRead = result.Count;
                while (!result.EndOfMessage)
                {
                    // Resize the buffer
                    byte[] temp = new byte[_buffer.Length * 2];
                    _buffer.CopyTo(temp, 0);
                    _buffer = temp;

                    // Append the next chunk
                    result = await WebSocket.ReceiveAsync(_buffer.AsMemory(bytesRead), cancellationToken);
                    bytesRead += result.Count;
                }

                return JsonSerializer.Deserialize<DeepgramLivestreamResult>(_buffer.AsSpan(0, bytesRead), DeepgramClient.DefaultJsonSerializerOptions);
            }
            catch (JsonException error)
            {
                JsonDocument document = JsonDocument.Parse(_buffer.AsMemory(0, (int)error.BytePositionInLine!.Value));
                throw new DeepgramException(document);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask<DeepgramLivestreamResult?> CloseAsync(CancellationToken cancellationToken = default)
        {
            if (WebSocket.State == WebSocketState.Closed)
            {
                throw new InvalidOperationException("WebSocket is already closed.");
            }

            await WebSocket.SendAsync(CloseMessage, WebSocketMessageType.Text, true, cancellationToken);
            return await ReceiveTranscriptionAsync(cancellationToken);
        }

        private async Task KeepAliveLoopAsync()
        {
            PeriodicTimer timer = new(KeepAliveInterval);
            while (await timer.WaitForNextTickAsync() && !_isDisposed && WebSocket.State == WebSocketState.Open)
            {
                await _semaphore.WaitAsync();

                // Everytime audio is sent, the keepalive timer is reset.
                if (DateTimeOffset.UtcNow - _lastKeepAlive <= KeepAliveInterval)
                {
                    continue;
                }

                await WebSocket.SendAsync(KeepAliveMessage, WebSocketMessageType.Text, true, CancellationToken.None);
                _lastKeepAlive = DateTimeOffset.UtcNow;
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    WebSocket.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}
