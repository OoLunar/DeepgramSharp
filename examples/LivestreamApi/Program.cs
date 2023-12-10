using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                Model = "nova-2"
            });

            SemaphoreSlim semaphoreSlim = new(0, 1);
            StringBuilder stringBuilder = new();
            transcriptionApi.OnTranscriptionReceived += (connection, transcriptionEventArgs) =>
            {
                stringBuilder.Append(transcriptionEventArgs.Result.Channel.Alternatives[0].Transcript);
                stringBuilder.Append(' ');
                Console.WriteLine("Transcript received.");
                return Task.CompletedTask;
            };

            transcriptionApi.OnErrorReceived += (connection, errorEventArgs) =>
            {
                Console.WriteLine(errorEventArgs.Error);
                return Task.CompletedTask;
            };

            transcriptionApi.OnClosed += (connection, closedEventArgs) =>
            {
                Console.WriteLine(stringBuilder.ToString());
                Console.WriteLine("Transcript finished.");
                semaphoreSlim.Release();
                return Task.CompletedTask;
            };

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

            await transcriptionApi.CloseAsync(); // Signal that we're done sending audio
            Console.WriteLine("Requesting closure...");
            await semaphoreSlim.WaitAsync();
            Debugger.Break();
        }
    }
}
