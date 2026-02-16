/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;

    [Muna.Embed(PredictorTag)]
    internal sealed class OpenAISpeechCreateTest : MonoBehaviour {

        private const string PredictorTag = "@kitten-ml/kitten-tts";

        private async void Start() {
            var openai = MunaUnity.Create().Beta.OpenAI;
            var response = await openai.Audio.Speech.Create(
                model: PredictorTag,
                input: @"This is a test for generating speech in Unity Engine.",
                voice: @"expr-voice-2-f",
                responseFormat: Beta.OpenAI.SpeechService.ResponseFormat.PCM
            );
            Debug.Log(response.MediaType);
        }
    }
}