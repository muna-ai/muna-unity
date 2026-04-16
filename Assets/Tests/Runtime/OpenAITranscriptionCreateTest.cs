/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    [Muna.Embed(ModelTag)]
    internal sealed class OpenAITranscriptionCreateTest : MonoBehaviour {

        private const string ModelTag = "@moonshine/moonshine-base";

        private async void Start() {
            var openai = MunaUnity.Create().Beta.OpenAI;
            using var file = await LoadStreamingAsset(@"librispeech_sample.wav");
            var response = await openai.Audio.Transcriptions.Create(
                model: ModelTag,
                file: file
            );
            Debug.Log(response.Text);
        }

        private static async Task<Stream> LoadStreamingAsset(string fileName) {
            var path = Path.Combine(Application.streamingAssetsPath, fileName);
            if (File.Exists(path))
                return File.OpenRead(path);
            var url = path.Contains("://") ? path : "file://" + path;
            using var request = UnityWebRequest.Get(url);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                throw new IOException($"Failed to load streaming asset '{fileName}': {request.error}");
            return new MemoryStream(request.downloadHandler.data);
        }
    }
}