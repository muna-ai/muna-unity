/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
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
                input: "What is the capital of France?"
            );
            Debug.Log(string.Join(",", response.Data[0].Floats));
        }
    }
}