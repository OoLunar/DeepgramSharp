using System;
using System.Globalization;
using DeepgramSharp.Entities;

namespace DeepgramSharp
{
    /// <summary>
    /// Represents a collection of options for the Deepgram livestream API.
    /// </summary>
    public sealed record DeepgramLivestreamOptionCollection : DeepgramOptionCollection
    {
        /// <summary>
        /// Indicates whether the streaming endpoint should send you updates to its transcription as more audio becomes available. When set to <see langword="true"/>, the streaming endpoint returns regular updates, which means transcription results will likely change for a period of time. By default, this flag is set to <see langword="false"/>. Learn more: <see href="https://developers.deepgram.com/docs/interim-results"/>
        /// </summary>
        public bool InterimResults { get => bool.Parse(_options[nameof(InterimResults)]); set => _options[nameof(InterimResults)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Indicates how long Deepgram will wait to detect whether a speaker has finished speaking (or paused for a significant period of time, indicating the completion of an idea). When Deepgram detects an endpoint, it assumes that no additional data will improve its prediction, so it immediately finalizes the result for the processed time range and returns the transcript with a speech_final parameter set to true.
        /// </summary>
        public TimeSpan Endpointing { get => TimeSpan.Parse(_options[nameof(Endpointing)], CultureInfo.InvariantCulture); set => _options[nameof(Endpointing)] = value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture); }

        /// <summary>
        /// Number of independent audio channels contained in submitted streaming audio. Only read when a value is provided for <see cref="Encoding"/>. Learn more: <see href="https://developers.deepgram.com/docs/channels"/>
        /// </summary>
        public int Channels { get => int.Parse(_options[nameof(Channels)], CultureInfo.InvariantCulture); set => _options[nameof(Channels)] = value.ToString(CultureInfo.InvariantCulture); }

        /// <summary>
        /// Sample rate of submitted streaming audio. Required (and only read) when a value is provided for <see cref="Encoding"/>. Learn more: <see href="https://developers.deepgram.com/docs/sample-rate"/>
        /// </summary>
        public int SampleRate { get => int.Parse(_options[nameof(SampleRate)], CultureInfo.InvariantCulture); set => _options[nameof(SampleRate)] = value.ToString(CultureInfo.InvariantCulture); }

        /// <summary>
        /// Expected encoding of the submitted streaming audio. If this parameter is set, <see cref="SampleRate"/> must also be specified. Learn more: <see href="https://developers.deepgram.com/docs/encoding"/>
        /// </summary>
        public DeepgramEncoding Encoding
        {
            get => _options[nameof(Encoding)] switch
            {
                "linear16" => DeepgramEncoding.Linear16,
                "mulaw" => DeepgramEncoding.Mulaw,
                "amr-nb" => DeepgramEncoding.AmrNb,
                "amr-wb" => DeepgramEncoding.AmrWb,
                "opus" => DeepgramEncoding.Opus,
                "speex" => DeepgramEncoding.Speex,
                _ => throw new NotImplementedException($"Encoding {_options[nameof(Encoding)]} has not been implemented.")
            }; set => _options[nameof(Encoding)] = ToSnakeCase(value.ToString()).Replace('_', '-');
        }
    }
}
