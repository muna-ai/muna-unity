/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;
    using Newtonsoft.Json;

    internal sealed class CreatePredictionOnBackgroundThreadTest : MonoBehaviour {

        [Header(@"Image")]
        [SerializeField] private Texture2D image;

        private async void Start() {
            var munaImage = image.ToImage();
            #if UNITY_6000_0_OR_NEWER
            await Awaitable.BackgroundThreadAsync();
            #else
            Debug.LogError(@"This test requires Unity 6.0+");
            return;
            #endif
            var muna = MunaUnity.Create();
            var prediction = await muna.Predictions.Create(
                tag: ImageClassifierTest.MobileNetv2Tag,
                inputs: new() { ["image"] = munaImage }
            );
            Debug.Log(JsonConvert.SerializeObject(prediction, formatting: Formatting.Indented));
        }
    }
}