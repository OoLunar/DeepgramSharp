# Using the Pre-recorded API

The pre-recorded API allows you to transcribe audio files that are stored on the internet or on your local machine. You can find more details about the pre-recorded API in the [official documentation](https://developers.deepgram.com/api-reference/deepgram-api#operation/transcribe).

1. **Initialize the Deepgram Client**

    You'll need to initialize the [`DeepgramClient`]("https://github.com/OoLunar/DeepgramSharp/blob/master/src/DeepgramClient.cs") with your API key. If desired, you can provide your own custom logger which will be helpful for debugging. By default, all clients use a `NullLogger`. Here's how you can do it:

    ```csharp
    var client = new DeepgramClient("your-api-key", myCustomLogger);
    ```

2. **Use the Pre-recorded API**

    Now you can use the [`DeepgramPrerecordedApi`](https://github.com/OoLunar/DeepgramSharp/blob/master/src/DeepgramPrerecordedApi.cs) class to interact with the pre-recorded API. As with all other API methods, use can pass a cancellation token to the method to cancel the operation. Here's an example of how you can do this:

    ```csharp
    var result = await client.PreRecordedApi.TranscribeAsync("url-to-your-audio-file", myCancellationToken);
    ```

    This will transcribe the audio file at the given url and return the result. Alternatively you can provide the audio file as a `Stream` object. Here's an example:

    ```csharp
    var prerecordedApi = new DeepgramPrerecordedApi(client);
    var result = await prerecordedApi.Transcribe(File.OpenRead("path-to-your-audio-file"));
    ```

3. **Using a custom model**

    By default, the pre-recorded API uses the `general` model. You can use a custom model by passing the model name within the `DeepgramPrerecordedApiOptions` object. Within the library are some default models that you can use for reference, but you can also use your own custom models. Here's an example of how you can do this:

    ```csharp
    var options = new DeepgramPrerecordedApiOptions
    {
        Tier = Nova2Tier.FullName, // Alternatively "my-custom-model"
        Language = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"),
        Punctuate = true
    };
    ```

4. **Handle the Result**

    The `Transcribe` method returns a nullable [`DeepgramTranscription`](https://github.com/OoLunar/DeepgramSharp/blob/master/Entities/DeepgramTranscription.cs) object. The transcription will contain metadata about the model used, the same amount of audio channels as the input data and most importantly, the recorded text. Here's an example of how you can handle the result:

    ```csharp
    if (result is not null)
    {
        Console.WriteLine(result.Text);
    }
    ```

Please replace `"your-api-key"` and `"path-to-your-audio-file"` with your actual API key and the path to your audio file.