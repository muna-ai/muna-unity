/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;
    using Newtonsoft.Json;

    [Muna.Embed(MobileNetv2Tag)]
    internal sealed class ImageClassifierTest : MonoBehaviour {

        [Header(@"Image")]
        [SerializeField] private Texture2D image;

        public const string MobileNetv2Tag = "@pytorch/mobilenet-v2";

        private async void Start() {
            var muna = MunaUnity.Create();
            var prediction = await muna.Predictions.Create(
                tag: MobileNetv2Tag,
                inputs: new() {
                    ["image"] = image.ToImage()
                }
            );
            Debug.Log(JsonConvert.SerializeObject(prediction, formatting: Formatting.Indented));
        }
    }
}