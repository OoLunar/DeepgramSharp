# DeepgramSharp

![GitHub Workflow Status (with event)](https://img.shields.io/github/actions/workflow/status/OoLunar/DeepgramSharp/build-commit.yml?style=for-the-badge&color=blueviolet)
 ![Discord](https://img.shields.io/discord/1070516376046944286?style=for-the-badge&color=blueviolet) ![Nuget](https://img.shields.io/nuget/dt/DeepgramSharp?style=for-the-badge&color=blueviolet)

An unofficial .NET API wrapper for Deepgram's automated speech recognition APIs.

DeepgramSharp is a C# client for the [Deepgram API](https://deepgram.com/). It provides interfaces for both the Livestream and Pre-recorded APIs. If you're struggling to use our library, you're more than welcome to open up an issue or join our [Discord](https://discord.gg/pvqh9Ud3Qv) server and ask for help in the #deepgram-sharp channel.

## Installation

DeepgramSharp is available on [NuGet](https://www.nuget.org/packages/DeepgramSharp/). You can install it using the following command:

```
dotnet add package DeepgramSharp
```

## Usage

In order to use the Deepgram API, you must first create a DeepgramClient. The DeepgramClient is the main entry point for the API. It provides methods for creating connections to the Livestream API and sending audio to the Pre-recorded API.

```csharp
var client = new DeepgramClient("my-api-key");
```

### Livestream API

The livestream API can be used for real-time transcription. It's useful for applications such as live captioning. The livestream API is a WebSocket API. The DeepgramSharp library provides a wrapper around the WebSocket API to make it easier to use.

```csharp
var livestreamApi = await client.CreateLivestreamAsync(new DeepgramLivestreamOptionCollection()
{
    Tier = Nova2Tier.FullName,
    Language = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"),
    Punctuate = true
});

DeepgramLivestreamResponse? response;
while((response = await livestreamApi.ReceiveTranscriptionAsync()) != null)
{
    switch (response.Type)
    {
        case DeepgramLivestreamResponseType.Transcription:
            Console.WriteLine($"Transcription: {response.Transcription.Text}")
            break;
        case DeepgramLivestreamResponseType.Error:
            Console.WriteLine($"Error: {response.Error.Message}")
            break;
        case DeepgramLivestreamResponseType.Close:
            Console.WriteLine($"Connection closed: {response.Close.Reason}")
            break;
    }
}
```

Refer to the [Using the Livestream API](https://www.forsaken-borders.net/DeepgramSharp/tutorials/live-api.md) tutorial for detailed usage instructions.

### Pre-recorded API

The pre-recorded API can be used for transcribing audio files. It's useful for applications such as voicemail transcription. The pre-recorded API is a REST API. The DeepgramSharp library provides a wrapper around the REST API to make it easier to use.

```csharp
var transcription = await client.SendAudioAsync(audioData, new DeepgramAudioOptionCollection()
{
    Tier = Nova2Tier.FullName,
    Language = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"),
    Punctuate = true
});
```

Refer to the [Using the Pre-recorded API](https://www.forsaken-borders.net/DeepgramSharp/tutorials/prerecorded-api.md) tutorial for detailed usage instructions.

## Examples

Examples can be found in the [examples](https://github.com/OoLunar/DeepgramSharp/tree/master/examples) directory. They demonstrate usage of both the Livestream and Pre-recorded APIs.

## API Documentation

The API documentation can be found [here](https://www.forsaken-borders.net/DeepgramSharp/). At the time of writing, entities are not documented due to the lack of documentation found on Deepgram's website. Contributions are welcome.

## Contributing

Contributions are welcome. If any warnings are found, try to resolve them before opening the PR. The only exception to this rule is warnings about missing documentation within the Deepgram entities - these can be ignored. If you're unsure about anything, feel free to join the Discord and ask questions in the #deepgram-sharp channel.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.