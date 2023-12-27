using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DeepgramSharp.Entities;
using Microsoft.Extensions.Logging;

namespace DeepgramSharp
{
    /// <summary>
    /// Represents a connection to the Deepgram livestream API.
    /// </summary>
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

        /// <summary>
        /// The <see cref="DeepgramClient"/> to use for requests.
        /// </summary>
        public DeepgramClient Client { get; init; } = client ?? throw new ArgumentNullException(nameof(client));

        /// <summary>
        /// The URI to use for requests.
        /// </summary>
        public Uri BaseUri { get; init; } = baseUri ?? DeepgramRoutes.LivestreamUri;

        /// <summary>
        /// The underlying <see cref="ClientWebSocket"/> used for communication with the Deepgram API.
        /// </summary>
        public ClientWebSocket WebSocket { get; init; } = new();

        /// <summary>
        /// The current state of the <see cref="WebSocket"/>.
        /// </summary>
        public WebSocketState State => WebSocket.State;

        private readonly ConcurrentQueue<DeepgramLivestreamResponse> _transcriptionQueue = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Pipe _pipe = new();
        private DateTimeOffset _lastKeepAlive = DateTimeOffset.Now;
        private bool _isDisposed;

        /// <summary>
        /// Connects to the Deepgram livestream API.
        /// </summary>
        /// <param name="options">The options to use for the connection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for the connection.</param>
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

        /// <summary>
        /// Sends audio to the Deepgram livestream API.
        /// </summary>
        /// <param name="audioFrames">A <see cref="ReadOnlyMemory{T}"/> containing the raw audio to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for the request.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public async ValueTask SendAudioAsync(ReadOnlyMemory<byte> audioFrames, CancellationToken cancellationToken = default)
        {
            if (WebSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("WebSocket is not open.");
            }

            await WebSocket.SendAsync(audioFrames, WebSocketMessageType.Binary, true, cancellationToken);
            _lastKeepAlive = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Attempts to receive the next transcription from the Deepgram livestream API.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for the request.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.</returns>
        public async ValueTask<DeepgramLivestreamResponse?> ReceiveTranscriptionAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                return _transcriptionQueue.TryDequeue(out DeepgramLivestreamResponse? response) ? response : null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Let's the Deepgram livestream API know that you are done sending audio.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for the request.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public async ValueTask RequestClosureAsync(CancellationToken cancellationToken = default)
        {
            if (WebSocket.State == WebSocketState.Closed)
            {
                throw new InvalidOperationException("WebSocket is already closed.");
            }

            await WebSocket.SendAsync(CloseMessage, WebSocketMessageType.Text, true, cancellationToken);
        }

        private async Task ReceiveTranscriptionLoopAsync()
        {
            while (!_isDisposed && WebSocket.State == WebSocketState.Open)
            {
                await _semaphore.WaitAsync();
                try
                {
                    Memory<byte> memory = _pipe.Writer.GetMemory(8192);
                    ValueWebSocketReceiveResult result = await WebSocket.ReceiveAsync(memory, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _transcriptionQueue.Enqueue(new DeepgramLivestreamResponse()
                        {
                            Type = DeepgramLivestreamResponseType.Closed
                        });

                        break;
                    }
                    else if (result.MessageType != WebSocketMessageType.Text)
                    {
                        throw new InvalidOperationException("WebSocket received unexpected and unknown binary data.");
                    }

                    _pipe.Writer.Advance(result.Count);
                    if (!result.EndOfMessage)
                    {
                        continue;
                    }

                    await _pipe.Writer.FlushAsync();
                    ParseTranscription();
                }
                catch (Exception error)
                {
                    Client.Logger.LogError(error, "An error occurred while receiving transcription data from the Deepgram API.");
                    _transcriptionQueue.Enqueue(new DeepgramLivestreamResponse()
                    {
                        Type = DeepgramLivestreamResponseType.Error,
                        Error = error
                    });

                    break;
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            void ParseTranscription()
            {
                while (_pipe.Reader.TryRead(out ReadResult readResult))
                {
                    ReadOnlySequence<byte> sequence = readResult.Buffer;
                    Utf8JsonReader reader = new(sequence, false, default);
                    DeepgramWebsocketMessageType type = default;
                    while (reader.Read())
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "type")
                        {
                            continue;
                        }

                        reader.Read();
                        if (reader.TokenType != JsonTokenType.String || !Enum.TryParse(reader.GetString(), out type))
                        {
                            Client.Logger.LogError("Received an unknown message type from the Deepgram API: {JsonDocument}", sequence);
                            return;
                        }

                        break;
                    }

                    // Fastpath for single segment
                    if (sequence.IsSingleSegment)
                    {
                        _transcriptionQueue.Enqueue(type switch
                        {
                            DeepgramWebsocketMessageType.Metadata => new DeepgramLivestreamResponse()
                            {
                                Type = DeepgramLivestreamResponseType.Metadata,
                                Metadata = JsonSerializer.Deserialize<DeepgramMetadata>(sequence.FirstSpan, DeepgramClient.DefaultJsonSerializerOptions)
                            },
                            DeepgramWebsocketMessageType.Results => new DeepgramLivestreamResponse()
                            {
                                Type = DeepgramLivestreamResponseType.Transcript,
                                Transcript = JsonSerializer.Deserialize<DeepgramLivestreamResult>(sequence.FirstSpan, DeepgramClient.DefaultJsonSerializerOptions)
                            },
                            _ => throw new InvalidOperationException($"Unknown payload type: {type}")
                        });
                    }
                    else if (readResult.IsCompleted)
                    {
                        _transcriptionQueue.Enqueue(type switch
                        {
                            DeepgramWebsocketMessageType.Metadata => new DeepgramLivestreamResponse()
                            {
                                Type = DeepgramLivestreamResponseType.Metadata,
                                Metadata = JsonSerializer.Deserialize<DeepgramMetadata>(_pipe.Reader.AsStream(), DeepgramClient.DefaultJsonSerializerOptions)
                            },
                            DeepgramWebsocketMessageType.Results => new DeepgramLivestreamResponse()
                            {
                                Type = DeepgramLivestreamResponseType.Transcript,
                                Transcript = JsonSerializer.Deserialize<DeepgramLivestreamResult>(_pipe.Reader.AsStream(), DeepgramClient.DefaultJsonSerializerOptions)
                            },
                            _ => throw new InvalidOperationException($"Unknown payload type: {type}")
                        });
                    }
                    else
                    {
                        _transcriptionQueue.Enqueue(type switch
                        {
                            DeepgramWebsocketMessageType.Metadata => new DeepgramLivestreamResponse()
                            {
                                Type = DeepgramLivestreamResponseType.Metadata,
                                Metadata = JsonSerializer.Deserialize<DeepgramMetadata>(sequence.ToArray(), DeepgramClient.DefaultJsonSerializerOptions)
                            },
                            DeepgramWebsocketMessageType.Results => new DeepgramLivestreamResponse()
                            {
                                Type = DeepgramLivestreamResponseType.Transcript,
                                Transcript = JsonSerializer.Deserialize<DeepgramLivestreamResult>(sequence.ToArray(), DeepgramClient.DefaultJsonSerializerOptions)
                            },
                            _ => throw new InvalidOperationException($"Unknown payload type: {type}")
                        });
                    }

                    _pipe.Reader.AdvanceTo(sequence.End);
                }
            }
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

        /// <inheritdoc cref="IDisposable.Dispose"/>
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
