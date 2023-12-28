# Using the livestream API

The livestream API allows you to transcribe audio streams in real-time. You can find more details about the livestream API in the [official documentation](https://developers.deepgram.com/api-reference/deepgram-api#operation/livestream).

1. **Initialize the Deepgram Client**

    You'll need to initialize the [`DeepgramClient`]("https://github.com/OoLunar/DeepgramSharp/blob/master/src/DeepgramClient.cs") with your API key. If desired, you can provide your own custom logger which will be helpful for debugging. By default, all clients use a `NullLogger`. Here's how you can do it:

    ```csharp
    var client = new DeepgramClient("your-api-key", myCustomLogger);
    ```

2. **Create a connection**

    Now you can use the `DeepgramLivestreamApi` class to interact with the livestream API. In order to translate audio in real time, you must first create a connection. By default, the library will return the [`DeepgramLivestreamApi`](https://github.com/OoLunar/DeepgramSharp/blob/master/src/DeepgramLivestreamApi.cs) object soon as audio can be sent. Additionally, the [`DeepgramLivestreamApi`](https://github.com/OoLunar/DeepgramSharp/blob/master/src/DeepgramLivestreamApi.cs) class will also handle the keep alive for you. As with all other API methods, use can pass a cancellation token to the method to cancel the operation. Here's an example of how you can do this:

    ```csharp
    var livestreamApi = await client.CreateLivestreamAsync(new DeepgramLivestreamOptionCollection()
    {
        Tier = Nova2Tier.FullName,
        Language = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"),
        Punctuate = true
    }, myCancellationToken);
    ```

3. **Sending audio data**

    You can send audio data to the livestream API using the `SendAudioAsync` method:
    ```csharp
    await livestreamApi.SendAudioAsync(audioData);
    ```

    Deepgram accepts numerous audio formats and will attempt to detect the format automatically. If there's a specific audio format you wish to use, you can specify the parameters within the [`DeepgramLivestreamOptionCollection`](https://github.com/OoLunar/DeepgramSharp/blob/master/src/DeepgramLivestreamOptionCollection.cs) object:

     ```csharp
     var livestreamApi = await client.CreateLivestreamAsync(new DeepgramLivestreamOptionCollection()
     {
        Tier = Nova2Tier.FullName,
        Language = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"),
        Punctuate = true,
        SampleRate = 48000,
        Channels = 2,
        Encoding = DeepgramEncoding.Opus
     }, myCancellationToken);
     ```

5. **Receiving results**

    You can receive results from the livestream API using the `ReceiveTranscriptionAsync` method. By default all results are returned in the order they were received. As with all other API methods, use can pass a cancellation token to the method to cancel the operation. Here's an example of how you can do this:

    ```csharp
    var result = await livestreamApi.ReceiveTranscriptionAsync(myCancellationToken);
    ```

    The result may have multiple uses. Sometimes it can contain metadata, other times it'll communicate an error has occured. By default your application should handle the transcription, error and close result types:

    ```csharp
    DeepgramLivestreamResponse response = await livestreamApi.ReceiveTranscriptionAsync(myCancellationToken);
    switch (response.Type)
    {
        case DeepgramLivestreamResponseType.Transcription:
            Console.WriteLine($"Transcription: {response.Transcription.Text}")
            break;
        case DeepgramLivestreamResponseType.Error:
            Console.WriteLine($"Error: {response.Error.Message}")
            break;
        case DeepgramLivestreamResponseType.Close:
            return;
    }
    ```

6. **Closing the livestream**

    You can let Deepgram know you're done sending audio through the [`RequestClosureAsync`](https://github.com/OoLunar/DeepgramSharp/blob/master/src/DeepgramLivestreamApi.cs) method:

    ```csharp
    await livestreamApi.RequestClosureAsync();
    ```

    Deepgram will finish processing the audio and return the final transcription. You can also close the livestream by disposing the [`DeepgramLivestreamApi`](https://github.com/OoLunar/DeepgramSharp/blob/master/src/DeepgramLivestreamApi.cs) object.