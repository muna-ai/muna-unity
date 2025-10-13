/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using System.Threading.Tasks;
    using UnityEngine;
    using global::Muna.Beta.OpenAI;

    [Muna.Embed(PredictorTag)]
    internal sealed class OpenAIChatCompletionStreamTest : MonoBehaviour {

        private const string PredictorTag = "@anon/gemma3-270m";

        [SerializeField, TextArea] private string prompt;

        private async void Start() {
            var openai = MunaUnity.Create().Beta.OpenAI;
            var stream = openai.Chat.Completions.Stream(
                model: PredictorTag,
                messages: new[] {
                    new ChatMessage {
                        Role = ChatMessage.ChatRole.User,
                        Content = prompt
                    }
                }
            );
            await foreach (var chunk in stream) {
                Debug.Log(chunk.Choices?[0]?.Delta?.Content);
                await Task.Yield();
            }
        }
    }
}