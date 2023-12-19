using System;
using System.Globalization;

namespace DeepgramSharp
{
    /// <summary>
    /// Represents a collection of options for the Deepgram prerecorded API.
    /// </summary>
    public sealed record DeepgramPrerecordedApiOptionCollection : DeepgramOptionCollection
    {
        /// <summary>
        /// Language Detection identifies the dominant language spoken in submitted audio. Learn more: <see href="https://developers.deepgram.com/docs/language-detection" />
        /// </summary>
        public bool DetectLanguage { get => bool.Parse(_options[nameof(DetectLanguage)]); set => _options[nameof(DetectLanguage)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Split audio into paragraphs. Default: <see langword="false"/>. Learn more: <see href="https://developers.deepgram.com/docs/paragraphs" />
        /// </summary>
        public bool Paragraphs { get => bool.Parse(_options[nameof(Paragraphs)]); set => _options[nameof(Paragraphs)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Summarize content. Default: <c>v2</c>. Learn more: <see href="https://developers.deepgram.com/docs/paragraphs" />
        /// </summary>
        public string Summarize { get => Uri.UnescapeDataString(_options[nameof(Summarize)]); set => _options[nameof(Summarize)] = Uri.EscapeDataString(value); }

        /// <summary>
        /// Identify and extract key topics. Default: <see langword="false"/>. Learn more: <see href="https://developers.deepgram.com/docs/topic-detection" />
        /// </summary>
        public bool DetectTopics { get => bool.Parse(_options[nameof(DetectTopics)]); set => _options[nameof(DetectTopics)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Segment speech into meaningful units based on gaps in speech. Default: <see langword="false"/>. Learn more: <see href="https://developers.deepgram.com/docs/utterances"/>
        /// </summary>
        public bool Utterances { get => bool.Parse(_options[nameof(Utterances)]); set => _options[nameof(Utterances)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Length of time in seconds used to split utterances. Default:
        /// </summary>
        public TimeSpan UttSplit { get => TimeSpan.FromMilliseconds(double.Parse(_options[nameof(UttSplit)], CultureInfo.InvariantCulture)); set => _options[nameof(UttSplit)] = value.TotalSeconds.ToString(CultureInfo.InvariantCulture); }

        /// <summary>
        /// Spoken measurements will be converted to their corresponding abbreviations. e.g., milligram to mg.
        /// </summary>
        public bool Measurements { get => bool.Parse(_options[nameof(Measurements)]); set => _options[nameof(Measurements)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Spoken dictation commands will be converted to their corresponding punctuation marks. e.g., <c>comma</c> to <c>,</c>
        /// </summary>
        public bool Dictation { get => bool.Parse(_options[nameof(Dictation)]); set => _options[nameof(Dictation)] = value.ToString().ToLowerInvariant(); }
    }
}
