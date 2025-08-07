/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [Muna.Embed(LlmTag)]
    internal sealed class TextGenerationTest : MonoBehaviour {

        private const string LlmTag = "@yusuf/llama-stream";

        private async void Start() {
            var muna = MunaUnity.Create();
            await foreach (var prediction in muna.Predictions.Stream(
                tag: LlmTag,
                inputs: new() {
                    [@"messages"] = new[] {
                        new JObject{
                            { "role", "user" },
                            { "content", "What is the capital of France?" }
                        }
                    }
                }
            )) {
                var chunk = prediction.results[0] as JObject;
                Debug.Log($"{JsonConvert.SerializeObject(chunk, Formatting.Indented)}");
                await Awaitable.WaitForSecondsAsync(0.01f);
            }
        }
    }
}