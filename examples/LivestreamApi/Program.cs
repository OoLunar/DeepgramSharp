using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DeepgramSharp.Entities;

namespace DeepgramSharp.Examples.LiveStreamApi
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            DeepgramClient client = new(Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY")!);
            DeepgramLivestreamApi transcriptionApi = await client.CreateLivestreamAsync(new()
            {
                Punctuate = true,
                Language = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"),
                Tier = "nova-2"
            });

            HttpClient httpClient = new();
            using HttpResponseMessage response = await httpClient.GetAsync("https://www2.cs.uic.edu/~i101/SoundFiles/taunt.wav", HttpCompletionOption.ResponseHeadersRead);
            using HttpContent content = response.Content;
            await using Stream stream = await content.ReadAsStreamAsync();
            byte[] buffer = new byte[1024 * 8]; // 8KB
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                await transcriptionApi.SendAudioAsync(buffer.AsMemory(0, bytesRead));
                await Task.Delay(200);
            }

            await transcriptionApi.RequestClosureAsync(); // Signal that we're done sending audio
            Console.WriteLine("Requesting closure...");

            DeepgramLivestreamResponse? livestreamResponse;
            while ((livestreamResponse = await transcriptionApi.ReceiveTranscriptionAsync()) is not null)
            {
                if (livestreamResponse.Type is DeepgramLivestreamResponseType.Transcript)
                {
                    Console.WriteLine(livestreamResponse.Transcript.Channel.Alternatives[0].Transcript);
                }
                else if (livestreamResponse.Type is DeepgramLivestreamResponseType.Error)
                {
                    Console.WriteLine(livestreamResponse.Error!.ToString());
                }
                else if (livestreamResponse.Type is DeepgramLivestreamResponseType.Closed)
                {
                    break;
                }
            }

            Debugger.Break();
        }
    }
}
