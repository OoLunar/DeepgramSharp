using System;
using System.Buffers;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DeepgramSharp.AsyncEvents;
using DeepgramSharp.Entities;
using DeepgramSharp.EventArgs;
using DeepgramSharp.Exceptions;
using Microsoft.Extensions.Logging;

namespace DeepgramSharp
{
    public sealed class DeepgramLivestreamApi(DeepgramClient client, Uri? baseUri = null) : IDisposable
    {
        private sealed class AuthenticatedMessageInvoker : HttpMessageInvoker
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

            public override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Client.SetDefaultHeaders(request);
                return base.Send(request, cancellationToken);
            }
        }

        private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(6);
        private static readonly byte[] KeepAliveMessage = JsonSerializer.SerializeToUtf8Bytes(new DeepgramWebsocketPayload()
        {
            Type = DeepgramPayloadType.KeepAlive.ToString()
        }, DeepgramClient.DefaultJsonSerializerOptions);

        private static readonly byte[] CloseMessage = JsonSerializer.SerializeToUtf8Bytes(new DeepgramWebsocketPayload()
        {
            Type = DeepgramPayloadType.CloseStream.ToString()
        }, DeepgramClient.DefaultJsonSerializerOptions);

        // Metadata
        public event AsyncEventHandler<DeepgramLivestreamApi, DeepgramLivestreamMetadataReceivedEventArgs> OnMetadataReceived { add => _metadataReceived.Register(value); remove => _metadataReceived.Unregister(value); }
        private readonly AsyncEvent<DeepgramLivestreamApi, DeepgramLivestreamMetadataReceivedEventArgs> _metadataReceived = new("METADATA_RECEIVED", EverythingWentWrongErrorHandler);

        public event AsyncEventHandler<DeepgramLivestreamApi, DeepgramLivestreamTranscriptReceivedEventArgs> OnTranscriptionReceived { add => _transcriptionReceived.Register(value); remove => _transcriptionReceived.Unregister(value); }
        private readonly AsyncEvent<DeepgramLivestreamApi, DeepgramLivestreamTranscriptReceivedEventArgs> _transcriptionReceived = new("TRANSCRIPTION_RECEIVED", EverythingWentWrongErrorHandler);

        public event AsyncEventHandler<DeepgramLivestreamApi, DeepgramLivestreamErrorEventArgs> OnErrorReceived { add => _errorReceived.Register(value); remove => _errorReceived.Unregister(value); }
        private readonly AsyncEvent<DeepgramLivestreamApi, DeepgramLivestreamErrorEventArgs> _errorReceived = new("ERROR_RECEIVED", EverythingWentWrongErrorHandler);

        public event AsyncEventHandler<DeepgramLivestreamApi, DeepgramLivestreamClosedEventArgs> OnClosed { add => _closed.Register(value); remove => _closed.Unregister(value); }
        private readonly AsyncEvent<DeepgramLivestreamApi, DeepgramLivestreamClosedEventArgs> _closed = new("CLOSED", EverythingWentWrongErrorHandler);

        public DeepgramClient Client { get; init; } = client ?? throw new ArgumentNullException(nameof(client));
        public Uri BaseUri { get; init; } = baseUri ?? DeepgramRoutes.LivestreamUri;
        public ClientWebSocket WebSocket { get; init; } = new();
        public WebSocketState State => WebSocket.State;

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private Memory<byte> _buffer = new byte[512];
        private DateTimeOffset _lastKeepAlive = DateTimeOffset.Now;
        private bool _isDisposed;

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
            _ = ReceiveTranscriptionLoopAsync();
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

        public async ValueTask CloseAsync(CancellationToken cancellationToken = default)
        {
            if (WebSocket.State == WebSocketState.Closed)
            {
                throw new InvalidOperationException("WebSocket is already closed.");
            }

            await WebSocket.SendAsync(CloseMessage, WebSocketMessageType.Text, true, cancellationToken);
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

        private async Task ReceiveTranscriptionLoopAsync()
        {
            while (!_isDisposed && WebSocket.State == WebSocketState.Open)
            {
                await _semaphore.WaitAsync();
                try
                {
                    ValueWebSocketReceiveResult result = await WebSocket.ReceiveAsync(_buffer, default);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _ = _closed.InvokeAsync(this, new());
                    }
                    else if (result.MessageType != WebSocketMessageType.Text)
                    {
                        throw new InvalidOperationException("WebSocket received unexpected message type.");
                    }

                    int bytesRead = result.Count;
                    while (!result.EndOfMessage)
                    {
                        // Resize the buffer
                        Memory<byte> temp = ArrayPool<byte>.Shared.Rent(_buffer.Length * 2);
                        _buffer.CopyTo(temp);
                        ArrayPool<byte>.Shared.Return(_buffer.ToArray(), true);
                        _buffer = temp;

                        // Append the next chunk
                        result = await WebSocket.ReceiveAsync(_buffer[bytesRead..], default);
                        bytesRead += result.Count;
                    }

                    JsonDocument jsonDocument = JsonDocument.Parse(_buffer[..bytesRead]);
                    if (!jsonDocument.RootElement.TryGetProperty("type", out JsonElement typeElement))
                    {
                        Client.Logger.LogError("Received an unknown message type from the Deepgram API: {JsonDocument}", jsonDocument.RootElement);
                    }

                    DeepgramWebsocketMessageType messageType = Enum.Parse<DeepgramWebsocketMessageType>(typeElement.GetString() ?? throw new InvalidOperationException("Received an unknown message type from the Deepgram API."), true);
                    _ = messageType switch
                    {
                        DeepgramWebsocketMessageType.Metadata => _metadataReceived.InvokeAsync(this, new()
                        {
                            Metadata = JsonSerializer.Deserialize<DeepgramMetadata>(_buffer.Span[..bytesRead], DeepgramClient.DefaultJsonSerializerOptions)!
                        }),
                        DeepgramWebsocketMessageType.Results => _transcriptionReceived.InvokeAsync(this, new()
                        {
                            Result = JsonSerializer.Deserialize<DeepgramLivestreamResult>(_buffer.Span[..bytesRead], DeepgramClient.DefaultJsonSerializerOptions)!
                        }),
                        _ => throw new InvalidOperationException("Received an unknown message type from the Deepgram API.")
                    };
                }
                catch (JsonException error) when (error.BytePositionInLine is not null)
                {
                    JsonDocument document = JsonDocument.Parse(_buffer[..(int)error.BytePositionInLine]);
                    _ = _errorReceived.InvokeAsync(this, new()
                    {
                        Error = new DeepgramWebsocketException(document)
                    });
                }
                finally
                {
                    _semaphore.Release();
                }
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

        private static void EverythingWentWrongErrorHandler<TArgs>(AsyncEvent<DeepgramLivestreamApi, TArgs> asyncEvent, Exception error, AsyncEventHandler<DeepgramLivestreamApi, TArgs> handler, DeepgramLivestreamApi sender, TArgs eventArgs) where TArgs : AsyncEventArgs => sender.Client.Logger.LogError(error, "Event handler '{Method}' for event {AsyncEvent} threw an unhandled exception.", handler.Method, asyncEvent.Name);
    }
}
