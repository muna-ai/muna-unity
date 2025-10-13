/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;

    [Muna.Embed(PredictorTag)]
    internal sealed class OpenAIEmbeddingCreateTest : MonoBehaviour {

        private const string PredictorTag = "@google/embedding-gemma";

        private async void Start() {
            var openai = MunaUnity.Create().Beta.OpenAI;
            var response = await openai.Embeddings.Create(
                model: PredictorTag,
                input: "What is the capital of France?",
                encodingFormat: Beta.OpenAI.EmbeddingService.EncodingFormat.Base64
            );
            Debug.Log(string.Join(",", response.data[0].embedding));
        }
    }
}