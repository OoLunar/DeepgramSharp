using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using DeepgramSharp.Entities;

namespace DeepgramSharp.Examples.PrerecordedApi
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            DeepgramClient client = new(Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY")!);
            DeepgramTranscription? transcription = await client.PreRecordedApi.TranscribeAsync(new Uri("https://www2.cs.uic.edu/~i101/SoundFiles/taunt.wav"), new()
            {
                Punctuate = true,
                Language = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"),
                Model = "nova-2"
            });

            if (transcription is null)
            {
                Console.WriteLine("Transcription failed.");
                return;
            }

            Console.WriteLine(transcription.Results.Channels[0].Alternatives[0].Transcript);

            // Take a break, look around a bit.
            Debugger.Break();
        }
    }
}
